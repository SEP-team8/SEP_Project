namespace webshop_back.DTOs
{
    public class CallbackDto
    {
        public string OrderId { get; set; } = "";
        public string Status { get; set; } = ""; // "PAID", "FAILED"
        public string? GlobalTransactionId { get; set; }
        public decimal? Amount { get; set; }
        public string? PaymentId { get; set; }
        public string? MerchantId { get; set; }
    }
}
