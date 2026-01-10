namespace webshop_back.Data.Models
{
    public class Order
    {
        public Guid OrderId { get; set; }

        public int UserId { get; set; }
        
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";

        public Guid MerchantId { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Initialized;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public ICollection<OrderItem>? Items { get; set; }
    }
}
