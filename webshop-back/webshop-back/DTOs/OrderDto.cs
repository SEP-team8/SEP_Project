using webshop_back.Data.Models;

namespace webshop_back.DTOs
{
    public class OrderDto
    {
        public Guid OrderId { get; set; }
        public Guid MerchantId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public OrderStatus Status { get; set; } = OrderStatus.Initialized;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public IEnumerable<OrderItemDto>? Items { get; set; }
    }
}
