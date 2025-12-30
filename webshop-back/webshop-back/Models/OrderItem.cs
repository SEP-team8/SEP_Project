namespace webshop_back.Models
{
    public class OrderItem
    {
        public string? ItemId { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
