using webshop_back.Data.Models;
using webshop_back.Service.Interfaces;

namespace webshop_back.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _config;

        public PaymentService(IConfiguration config)
        {
            _config = config;
        }

        public PaymentInitResponse InitializePaymentToAcquirer(PaymentInitRequest req, Order order)
        {
            // Simple stub implementation: generate PaymentId and PaymentUrl pointing to local PSP simulator
            var paymentId = $"P-{Guid.NewGuid():N}";
            var amount = req.AMOUNT;
            var currency = req.CURRENCY;

            // base url for PSP simulator (configurable)
            var simulatorBase = _config["Psp:SimulatorBaseUrl"]?.TrimEnd('/') ?? "https://localhost:5001";

            // Build PSP simulate URL — PSP will render form and post back to callback/complete endpoints
            var paymentUrl = $"{simulatorBase}/psp/simulate-payment?paymentId={Uri.EscapeDataString(paymentId)}&successUrl={Uri.EscapeDataString(req.SUCCESS_URL)}&failedUrl={Uri.EscapeDataString(req.FAILED_URL)}";

            // For QR flow, provide some dummy payload
            var qrPayload = $"PAYMENT:{paymentId};AMOUNT:{amount};CUR:{currency}";

            return new PaymentInitResponse
            {
                PaymentId = paymentId,
                PaymentUrl = paymentUrl,
                QrPayload = qrPayload,
                Amount = amount,
                Currency = currency
            };
        }
    }
}
