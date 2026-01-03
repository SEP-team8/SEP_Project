namespace webshop_back.DTOs
{
    public class VehicleDto
    {
        public int Id { get; set; }
        public string Make { get; set; } = "";
        public string Model { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public string? ImageBase64 { get; set; }
    }
}
