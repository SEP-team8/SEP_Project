using Microsoft.EntityFrameworkCore;
using PSPbackend.Context;
using PSPbackend.DTOs.Crypto;
using PSPbackend.Models;
using PSPbackend.Models.Enums;
using System.Net.Http.Json;

namespace PSPbackend.Services
{
    public class PaymentMethodRouter : IPaymentMethodRouter
    {
        private readonly IBankClient _bank;
        private readonly IPayPalServiceClient _paypal;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        private readonly PspDbContext _db;

        public PaymentMethodRouter(
            IBankClient bank,
            IPayPalServiceClient paypal,
            IHttpClientFactory httpFactory,
            IConfiguration config,
            PspDbContext db)
        {
            _bank = bank;
            _paypal = paypal;
            _httpFactory = httpFactory;
            _config = config;
            _db = db;
        }

        public async Task<string> RouteAsync(PaymentTransaction transaction, Merchant merchant, CancellationToken ct)
        {
            return transaction.PaymentMethod switch
            {
                PaymentMethodType.Card or PaymentMethodType.QrCode => await RouteToBankAsync(transaction, merchant, ct),
                PaymentMethodType.PayPal => await RouteToPayPalAsync(transaction, ct),
                PaymentMethodType.Crypto => await RouteCryptoAsync(transaction, merchant, ct),
                _ => throw new NotSupportedException($"Payment method {transaction.PaymentMethod} is not supported.")
            };
        }
        private async Task<string> RouteToBankAsync(PaymentTransaction transaction, Merchant merchant, CancellationToken ct)
        {
            var bankResponse = await _bank.CreatePaymentAsync(transaction, merchant.BankMerchantId, ct);
            return bankResponse.PaymentRequestUrl;
        }

        private async Task<string> RouteToPayPalAsync(PaymentTransaction transaction, CancellationToken ct)
        {
            var baseReturnUrl = _config["PayPal:ReturnUrl"]!;
            var baseCancelUrl = _config["PayPal:CancelUrl"]!;

            var returnUrl = $"{baseReturnUrl}?merchantId={transaction.MerchantId}" +
                            $"&stan={Uri.EscapeDataString(transaction.Stan)}" +
                            $"&pspTimestamp={Uri.EscapeDataString(transaction.PspTimestamp.ToString("o"))}";

            var cancelUrl = $"{baseCancelUrl}?merchantId={transaction.MerchantId}" +
                            $"&stan={Uri.EscapeDataString(transaction.Stan)}" +
                            $"&pspTimestamp={Uri.EscapeDataString(transaction.PspTimestamp.ToString("o"))}";

            return await _paypal.CreateOrderAsync(transaction, returnUrl, cancelUrl, ct);
        }

        private async Task<string> RouteCryptoAsync(PaymentTransaction transaction, Merchant merchant, CancellationToken ct)
        {
            var cryptoBase = _config["CryptoService:BaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(cryptoBase))
                throw new InvalidOperationException("CryptoService not configured");

            var client = _httpFactory.CreateClient();
            client.BaseAddress = new Uri(cryptoBase);

            var cryptoReq = new
            {
                merchantId = transaction.MerchantId,
                FiatAmount = transaction.Amount,
                currency = (int)transaction.Currency,
                stan = transaction.Stan,
                pspTimestamp = transaction.PspTimestamp
            };

            using var httpReq = new HttpRequestMessage(HttpMethod.Post, "/crypto/payments")
            {
                Content = JsonContent.Create(cryptoReq)
            };

            var resp = await client.SendAsync(httpReq, ct);
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"CryptoService error: {(int)resp.StatusCode} {resp.ReasonPhrase}");

            var body = await resp.Content.ReadFromJsonAsync<CreateCryptoPaymentResponse>(cancellationToken: ct);
            if (body == null)
                throw new InvalidOperationException("CryptoService returned invalid response.");

            transaction.CryptoPaymentId = body.PaymentId;
            transaction.CryptoAddress = body.EthAddress;
            transaction.CryptoAmount = body.EthAmount;
            transaction.CryptoChainId = body.ChainId;
            transaction.Currency = Currency.ETH;
            transaction.Status = TransactionStatus.Initialized;

            await _db.SaveChangesAsync(ct);

            var frontendBase = _config["PspFrontend:BaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(frontendBase))
                throw new InvalidOperationException("PSP frontend not configured");

            var paymentPage =
                $"{frontendBase}/payCrypto" +
                $"?paymentId={body.PaymentId}" +
                $"&merchantId={transaction.MerchantId}" +
                $"&stan={transaction.Stan}";

            return paymentPage;
        }
    }
}