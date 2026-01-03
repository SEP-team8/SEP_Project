namespace webshop_back.Data.Models
{
    public class PaymentInitResponse
    {
        public string PaymentId { get; set; } = "";
        public string? PaymentUrl { get; set; } // redirect to bank page
        public string? QrPayload { get; set; } // for QR flow
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
    }
}
