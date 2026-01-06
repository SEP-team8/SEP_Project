namespace PSPbackend.DTOs
{
    public class PaymentRequest
    {
        public string? PurchaseId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? Merchant { get; set; }
    }
}
