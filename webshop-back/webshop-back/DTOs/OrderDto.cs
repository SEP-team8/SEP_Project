namespace webshop_back.DTOs
{
    public class OrderDto
    {
        public string OrderId { get; set; } = "";
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public string Status { get; set; } = "Initialized";
        public DateTime CreatedAt { get; set; }
    }
}
