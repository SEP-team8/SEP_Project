namespace webshop_back.Models
{
    public class Order
    {
        public string Id { get; set; } = null!;
        public string MerchantOrderId { get; set; } = null!;
        public string? UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public string Status { get; set; } = "CREATED";
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
