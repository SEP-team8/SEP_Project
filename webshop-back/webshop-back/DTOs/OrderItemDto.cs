namespace webshop_back.DTOs
{
    public class OrderItemDto
    {
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = "";
        public decimal PricePerDay { get; set; }
        public int Days { get; set; }
        public decimal Total { get; set; }
    }
}
