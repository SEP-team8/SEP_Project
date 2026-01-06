using webshop_back.Data.Models;
using webshop_back.Service.Interfaces;

namespace webshop_back.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public PaymentInitResponse InitializePaymentToAcquirer(
            PaymentInitRequest req,
            Order order)
        {
            var paymentId = $"P-{Guid.NewGuid():N}";
            var amount = req.AMOUNT;
            var currency = req.CURRENCY;

            // ⬇⬇⬇ uzmi BASE URL iz trenutnog requesta
            var request = _httpContextAccessor.HttpContext!.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var paymentUrl =
                $"{baseUrl}/psp/simulate-payment" +
                $"?paymentId={Uri.EscapeDataString(paymentId)}" +
                $"&successUrl={Uri.EscapeDataString(req.SUCCESS_URL)}" +
                $"&failedUrl={Uri.EscapeDataString(req.FAILED_URL)}";

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
