namespace webshop_back.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Make { get; set; } = "";
        public string Model { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; } // per day
        public string? Image { get; set; }
    }
}
