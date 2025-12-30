namespace webshop_back.Models
{
    public class Vehicle
    {
        public string Id { get; set; } = null!;
        public string Make { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Class { get; set; } = null!;
        public decimal PricePerDay { get; set; }
    }
}
