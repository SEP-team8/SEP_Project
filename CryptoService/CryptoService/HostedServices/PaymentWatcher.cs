using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CryptoService.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using System.Net.Http;

namespace CryptoService.HostedServices
{
    public class PaymentWatcher : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<PaymentWatcher> _logger;
        private readonly string _rpcUrl;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;

        public PaymentWatcher(
            IServiceProvider services,
            IConfiguration config,
            ILogger<PaymentWatcher> logger,
            IHttpClientFactory httpFactory)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpFactory = httpFactory ?? throw new ArgumentNullException(nameof(httpFactory));

            _rpcUrl = _config["Ethereum:RpcUrl"] ?? throw new InvalidOperationException("Ethereum:RpcUrl not configured");
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            _logger.LogInformation("PaymentWatcher started");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<CryptoDbContext>();
                    var web3 = new Web3(_rpcUrl);

                    // Fetch pending payments that are not expired
                    var pending = await db.CryptoPayments
                        .Where(p => p.Status == CryptoService.Models.CryptoPaymentStatus.Pending && p.ExpiresAt > DateTime.UtcNow)
                        .ToListAsync(token);

                    foreach (var p in pending)
                    {
                        try
                        {
                            var balance = await web3.Eth.GetBalance.SendRequestAsync(p.EthAddress);
                            var expected = BigInteger.Parse(p.AmountWei);

                            if (balance >= expected)
                            {
                                // update entity
                                p.Status = CryptoService.Models.CryptoPaymentStatus.Detected;
                                p.TransactionHash ??= null; // may remain null if indexer not used

                                await db.SaveChangesAsync(token);
                                _logger.LogInformation("Detected funds for payment {PaymentId} on address {Address}", p.Id, p.EthAddress);

                                // Notify PSP about the detected payment (async)
                                await NotifyPspAsync(p, token);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Watcher error for payment {PaymentId}", p.Id);
                        }
                    }

                    // Optionally: check Detected ones that have TransactionHash for confirmations here

                }
                catch (Exception outerEx)
                {
                    _logger.LogError(outerEx, "PaymentWatcher main loop error");
                }

                // wait before next iteration
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), token);
                }
                catch (TaskCanceledException) { /* exit gracefully */ }
            }
        }

        // NOTE: We accept the same model type used by DbContext (fully-qualified Models.CryptoPayment).
        private async Task NotifyPspAsync(CryptoService.Models.CryptoPayment payment, CancellationToken ct)
        {
            // Read PSP config
            var callback = _config["Psp:CallbackUrl"];
            var secret = _config["Psp:SharedSecret"];
            var pspId = _config["Psp:PspId"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(callback) || string.IsNullOrWhiteSpace(secret))
            {
                _logger.LogWarning("Psp:CallbackUrl or Psp:SharedSecret not configured - skipping notify for payment {PaymentId}", payment.Id);
                return;
            }

            // Build payload matching PSP's CryptoPaymentNotificationDto shape (primitive JSON, no PSP types referenced)
            var payloadObj = new
            {
                MerchantID = payment.MerchantId ?? Guid.Empty,
                Stan = payment.Stan ?? string.Empty,
                PspTimestamp = payment.PspTimestamp ?? DateTime.UtcNow,
                Status = 0, // 0 == Success in PSP TransactionStatus enum (keep numeric to avoid referencing PSP types)
                TransactionHash = payment.TransactionHash,
                CryptoPaymentId = payment.Id,
                GlobalTransactionId = Guid.NewGuid(), // optional; adjust if you have a mapping
                AcquirerTimestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(payloadObj);

            var signature = CreateHmacSignature(json, secret);

            try
            {
                var client = _httpFactory.CreateClient();
                using var req = new HttpRequestMessage(HttpMethod.Post, callback)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                req.Headers.Add("Signature", signature);
                if (!string.IsNullOrEmpty(pspId))
                    req.Headers.Add("PspID", pspId);

                _logger.LogInformation("Sending PSP callback for payment {PaymentId} to {CallbackUrl}", payment.Id, callback);

                var resp = await client.SendAsync(req, ct);

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("PSP callback returned status {StatusCode} for payment {PaymentId}. Response: {RespText}", resp.StatusCode, payment.Id, await resp.Content.ReadAsStringAsync(ct));
                    // Optional: implement retry/backoff logic here
                }
                else
                {
                    _logger.LogInformation("PSP notified successfully for payment {PaymentId}", payment.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify PSP for payment {PaymentId}", payment.Id);
                // Optional: queue for retry
            }
        }

        private static string CreateHmacSignature(string payload, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
