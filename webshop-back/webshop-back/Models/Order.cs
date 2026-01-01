namespace webshop_back.Models
{
    public class Order
    {
        public string OrderId { get; set; } = "";
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public string Status { get; set; } = "Initialized";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
