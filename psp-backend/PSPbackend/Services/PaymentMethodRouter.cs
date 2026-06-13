using PSPbackend.Context;
using PSPbackend.Context;
using PSPbackend.DTOs.Crypto;
using PSPbackend.Models;
using PSPbackend.Models.Enums;
using System.Net.Http.Json;
using System.Text.Json;

namespace PSPbackend.Services
{
    public class PaymentMethodRouter : IPaymentMethodRouter
    {
        private readonly IBankClient _bank;
        private readonly IPayPalServiceClient _paypal;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        private readonly PspDbContext _pspDbContext;

        public PaymentMethodRouter(
            IBankClient bank,
            IPayPalServiceClient paypal,
            IHttpClientFactory httpFactory,
            IConfiguration config,
            PspDbContext pspDbContext)
        {
            _bank = bank;
            _paypal = paypal;
            _httpFactory = httpFactory;
            _config = config;
            _pspDbContext = pspDbContext;
        }

        public async Task<string> RouteAsync(PaymentTransaction transaction, Merchant merchant, CancellationToken ct)
        {
            return transaction.PaymentMethod switch
            {
                PaymentMethodType.Card or PaymentMethodType.QrCode => await RouteToBankAsync(transaction, merchant, ct),
                PaymentMethodType.PayPal => await RouteToPayPalAsync(transaction, ct),
                PaymentMethodType.Crypto => await RouteToCryptoAsync(transaction, ct),
                _ => throw new NotSupportedException($"Payment method '{transaction.PaymentMethod}' is not yet supported.")
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

        private async Task<string> RouteToCryptoAsync(PaymentTransaction transaction, CancellationToken ct)
        {
            var cryptoBase = _config["CryptoService:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("CryptoService:BaseUrl missing");
            var pspFrontendBase = _config["PspFrontend:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("PspFrontend:BaseUrl missing");

            var client = _httpFactory.CreateClient();
            client.BaseAddress = new Uri(cryptoBase);

            var request = new CreateCryptoPaymentRequest
            {
                MerchantId = transaction.MerchantId,
                FiatAmount = transaction.Amount,
                Currency = (int)transaction.Currency,
                Stan = transaction.Stan,
                PspTimestamp = transaction.PspTimestamp
            };

            var response = await client.PostAsJsonAsync("/payments", request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException($"CryptoService error: {response.StatusCode} - {errorBody}");
            }

            var responseText = await response.Content.ReadAsStringAsync(ct);
            CreateCryptoPaymentResponse? body;
            try
            {
                body = JsonSerializer.Deserialize<CreateCryptoPaymentResponse>(responseText);
            }
            catch (JsonException)
            {
                throw new InvalidOperationException($"CryptoService returned invalid JSON response: {responseText}");
            }

            if (body == null)
            {
                throw new InvalidOperationException($"CryptoService returned invalid response body: {responseText}");
            }

            transaction.CryptoPaymentId = body.PaymentId;
            transaction.CryptoAddress = body.EthAddress;
            transaction.CryptoAmount = body.EthAmount;
            transaction.CryptoChainId = body.ChainId;
            transaction.Status = TransactionStatus.Initialized;

            await _pspDbContext.SaveChangesAsync(ct);

            return $"{pspFrontendBase}/payCrypto?paymentId={body.PaymentId}";
        }
    }
}
