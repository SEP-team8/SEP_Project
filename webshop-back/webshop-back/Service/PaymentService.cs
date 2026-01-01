using webshop_back.Models;

namespace webshop_back.Service
{
    public class PaymentService
    {
        // Simulate generating payment id and payment url (bank)
        public PaymentInitResponse InitializePayment(PaymentInitRequest req)
        {
            var pid = "PAY-" + Guid.NewGuid().ToString("N").Substring(0, 12);
            var response = new PaymentInitResponse
            {
                PaymentId = pid,
                Amount = req.AMOUNT,
                Currency = req.CURRENCY
            };

            if ((req.Method ?? "card").ToLower() == "card")
            {
                // generate a simulated bank payment url that the frontend will redirect to
                // In real world: PSP requests bank, bank returns PAYMENT_URL. Here we simulate that.
                response.PaymentUrl = $"https://localhost:5001/psp/simulate-payment?paymentId={pid}&successUrl={Uri.EscapeDataString(req.SUCCESS_URL)}&failedUrl={Uri.EscapeDataString(req.FAILED_URL)}";
            }
            else
            {
                // QR payload: include amount, currency, merchant account — minimal example
                response.QrPayload = $"PAY|{req.CURRENCY}|{req.AMOUNT}|ME{req.MERCHANT_ID}|ORDER{req.MERCHANT_ORDER_ID}";
            }

            return response;
        }
    }
}
