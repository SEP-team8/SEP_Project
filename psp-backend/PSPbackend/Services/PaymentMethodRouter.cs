using PSPbackend.Models;
using PSPbackend.Models.Enums;

namespace PSPbackend.Services
{
    public class PaymentMethodRouter : IPaymentMethodRouter
    {
        private readonly IBankClient _bank;
        private readonly IPayPalServiceClient _paypal;
        private readonly IConfiguration _config;

        public PaymentMethodRouter(IBankClient bank, IPayPalServiceClient paypal, IConfiguration config)
        {
            _bank = bank;
            _paypal = paypal;
            _config = config;
        }

        public async Task<string> RouteAsync(PaymentTransaction transaction, Merchant merchant, CancellationToken ct)
        {
            return transaction.PaymentMethod switch
            {
                PaymentMethodType.Card or PaymentMethodType.QrCode => await RouteToBankAsync(transaction, merchant, ct),
                PaymentMethodType.PayPal => await RouteToPayPalAsync(transaction, ct),
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
    }
}
