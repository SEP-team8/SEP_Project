namespace webshop_back.Data.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Make { get; set; } = "";
        public string Model { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; } // per day
        public byte[]? Image { get; set; }
    }
}
