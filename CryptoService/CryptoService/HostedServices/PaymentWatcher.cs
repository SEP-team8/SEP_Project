using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CryptoService.Persistance;
using Microsoft.EntityFrameworkCore;
using Nethereum.Web3;

namespace CryptoService.HostedServices
{
    public class PaymentWatcher : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<PaymentWatcher> _logger;
        private readonly string _rpcUrl;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        private readonly int _confirmationsRequired;
        private readonly JsonSerializerOptions _jsonOptions;

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
            _confirmationsRequired = int.Parse(_config["Ethereum:ConfirmationsRequired"] ?? "1");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false
            };
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            _logger.LogInformation("PaymentWatcher started (rpc={Rpc})", _rpcUrl);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<CryptoDbContext>();
                    var web3 = new Web3(_rpcUrl);

                    // Fetch payments we should check:
                    var toCheck = await db.CryptoPayments
                        .Where(p => (p.Status == CryptoService.Models.CryptoPaymentStatus.Pending || p.Status == CryptoService.Models.CryptoPaymentStatus.Detected)
                                    && p.ExpiresAt > DateTime.UtcNow)
                        .ToListAsync(token);

                    if (toCheck.Count > 0)
                        _logger.LogInformation("Watcher found {Count} payments to check", toCheck.Count);

                    foreach (var p in toCheck)
                    {
                        try
                        {
                            var expected = BigInteger.Parse(p.AmountWei ?? "0");

                            // If we have a tx hash, prefer verifying transaction + confirmations
                            if (!string.IsNullOrWhiteSpace(p.TransactionHash))
                            {
                                var txHash = p.TransactionHash.StartsWith("0x") ? p.TransactionHash : "0x" + p.TransactionHash;

                                var tx = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash);
                                var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);

                                if (receipt != null && receipt.BlockNumber != null)
                                {
                                    var latest = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                                    var confirmations = (long)(latest.Value - receipt.BlockNumber.Value) + 1;

                                    if (tx == null)
                                        tx = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash);

                                    var toAddress = (tx?.To ?? string.Empty).ToLowerInvariant();
                                    var expectedTo = (p.EthAddress ?? string.Empty).ToLowerInvariant();
                                    var txValue = tx?.Value ?? BigInteger.Zero;

                                    var validTo = toAddress == expectedTo;
                                    var validValue = txValue >= expected;

                                    if (validTo && validValue && confirmations >= _confirmationsRequired)
                                    {
                                        p.Status = CryptoService.Models.CryptoPaymentStatus.Confirmed;
                                        p.TransactionHash = txHash;
                                        // do not overwrite original PspTimestamp; keep the one provided by PSP when creating
                                        p.PspTimestamp = p.PspTimestamp ?? DateTime.UtcNow;

                                        await db.SaveChangesAsync(token);

                                        _logger.LogInformation("Payment {PaymentId} confirmed (tx {Tx}). Confirmations: {Conf}", p.Id, txHash, confirmations);

                                        // Notify PSP only when confirmed
                                        await NotifyPspAsync(p, token);
                                    }
                                    else if (validTo && validValue)
                                    {
                                        p.Status = CryptoService.Models.CryptoPaymentStatus.Detected;
                                        p.TransactionHash = txHash;
                                        await db.SaveChangesAsync(token);
                                        _logger.LogInformation("Payment {PaymentId} detected (tx {Tx}) but waiting confirmations ({Conf}).", p.Id, txHash, confirmations);
                                    }
                                    else
                                    {
                                        p.TransactionHash = txHash;
                                        await db.SaveChangesAsync(token);
                                        _logger.LogWarning("Payment {PaymentId} has tx {Tx} but doesn't match expected to/value.", p.Id, txHash);
                                    }
                                }
                                else
                                {
                                    // fallback: check address balance
                                    var balance = await web3.Eth.GetBalance.SendRequestAsync(p.EthAddress);
                                    if (balance >= expected)
                                    {
                                        p.Status = CryptoService.Models.CryptoPaymentStatus.Detected;
                                        await db.SaveChangesAsync(token);
                                        _logger.LogInformation("Payment {PaymentId} balance >= expected but receipt missing. Marked Detected.", p.Id);
                                    }
                                    else
                                    {
                                        _logger.LogDebug("Payment {PaymentId} tx {Tx} not mined yet.", p.Id, p.TransactionHash);
                                    }
                                }
                            }
                            else
                            {
                                // No txHash yet — check balance on address
                                var balance = await web3.Eth.GetBalance.SendRequestAsync(p.EthAddress);
                                if (balance >= expected)
                                {
                                    p.Status = CryptoService.Models.CryptoPaymentStatus.Detected;
                                    await db.SaveChangesAsync(token);
                                    _logger.LogInformation("Detected funds on address {Address} for payment {PaymentId} but no txHash provided.", p.EthAddress, p.Id);
                                    // NOTE: we wait for txHash/receipt before notifying PSP
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Watcher error for payment {PaymentId}", p.Id);
                        }
                    }

                }
                catch (Exception outerEx)
                {
                    _logger.LogError(outerEx, "PaymentWatcher main loop error");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), token);
                }
                catch (TaskCanceledException) { /* exit */ }
            }
        }

        private async Task NotifyPspAsync(CryptoService.Models.CryptoPayment payment, CancellationToken ct)
        {
            var callback = _config["Psp:CallbackUrl"];
            var secret = _config["Psp:SharedSecret"];
            var pspId = _config["Psp:PspId"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(callback) || string.IsNullOrWhiteSpace(secret))
            {
                _logger.LogWarning("Psp:CallbackUrl or Psp:SharedSecret not configured - skipping notify for payment {PaymentId}", payment.Id);
                return;
            }

            // Build payload with stable property order (as declared)
            var payloadObj = new
            {
                MerchantID = payment.MerchantId ?? Guid.Empty,
                Stan = payment.Stan ?? string.Empty,
                PspTimestamp = payment.PspTimestamp ?? DateTime.UtcNow,
                Status = 0, // 0 == Success
                TransactionHash = payment.TransactionHash,
                CryptoPaymentId = payment.Id,
                GlobalTransactionId = Guid.NewGuid(),
                AcquirerTimestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(payloadObj, _jsonOptions);
            var signature = CreateHmacSignature(json, secret);

            _logger.LogInformation("NotifyPSP preparing callback for payment {PaymentId} -> {CallbackUrl}", payment.Id, callback);
            _logger.LogDebug("NotifyPSP payload: {Json}", json);
            _logger.LogDebug("NotifyPSP signature: {Sig}", signature);

            // retry logic
            var maxAttempts = 3;
            var attempt = 0;
            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            while (attempt < maxAttempts)
            {
                attempt++;
                try
                {
                    using var req = new HttpRequestMessage(HttpMethod.Post, callback)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };

                    req.Headers.Add("Signature", signature);
                    if (!string.IsNullOrEmpty(pspId))
                        req.Headers.Add("PspID", pspId);

                    _logger.LogInformation("NotifyPSP attempt {Attempt}/{Max} for payment {PaymentId}", attempt, maxAttempts, payment.Id);

                    var resp = await client.SendAsync(req, ct);
                    var respText = await resp.Content.ReadAsStringAsync(ct);

                    if (!resp.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("PSP callback returned status {StatusCode} for payment {PaymentId}. Response: {RespText}", resp.StatusCode, payment.Id, respText);
                        // exponential backoff before retrying
                        if (attempt < maxAttempts)
                        {
                            var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                            _logger.LogInformation("Waiting {Delay}s before next notify attempt", delay.TotalSeconds);
                            await Task.Delay(delay, ct);
                            continue;
                        }
                        else
                        {
                            _logger.LogError("PSP callback failed after {Attempts} attempts for payment {PaymentId}", attempt, payment.Id);
                            break;
                        }
                    }
                    else
                    {
                        _logger.LogInformation("PSP notified successfully for payment {PaymentId}. PSP response: {RespText}", payment.Id, respText);
                        break;
                    }
                }
                catch (TaskCanceledException tce) when (!ct.IsCancellationRequested)
                {
                    _logger.LogWarning(tce, "Timeout when notifying PSP (attempt {Attempt}) for payment {PaymentId}", attempt, payment.Id);
                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception when notifying PSP (attempt {Attempt}) for payment {PaymentId}", attempt, payment.Id);
                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct);
                        continue;
                    }
                }
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
