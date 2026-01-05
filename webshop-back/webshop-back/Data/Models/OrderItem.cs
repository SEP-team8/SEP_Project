namespace webshop_back.Data.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public string OrderId { get; set; } = "";

        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = "";
        public decimal PricePerDay { get; set; }
        public int Days { get; set; }
        public decimal Total => PricePerDay * Days;

        public Order? Order { get; set; }
    }
}
