namespace webshop_back.Models
{
    public class Payment
    {
        public string Id { get; set; } = null!;
        public string OrderId { get; set; } = null!;
        public string? PspPaymentId { get; set; }
        public string? PaymentUrl { get; set; }
        public string? Stan { get; set; }
        public string Status { get; set; } = "INIT";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
