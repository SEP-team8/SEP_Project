namespace webshop_back.Data.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public Guid OrderId { get; set; }
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = "";
        public decimal PricePerDay { get; set; }
        public int Days { get; set; }

        public Order? Order { get; set; }
    }
}
