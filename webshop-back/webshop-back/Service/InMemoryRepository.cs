using webshop_back.Models;

namespace webshop_back.Service
{
    public class InMemoryRepository : IRepository
    {
        private readonly List<Vehicle> _vehicles = new();
        private readonly List<Order> _orders = new();

        public InMemoryRepository()
        {
            _vehicles.Add(new Vehicle
            {
                Id = 1,
                Make = "Toyota",
                Model = "Corolla",
                Description = "Compact",
                Price = 35,
                Image = "../images/toyota-corolla-hybrid.jpg"
            });

            _vehicles.Add(new Vehicle
            {
                Id = 2,
                Make = "Skoda",
                Model = "Octavia",
                Description = "Family",
                Price = 45,
                Image = "../images/2024-Skoda-Octavia-RS.jpg"
            });

            _vehicles.Add(new Vehicle
            {
                Id = 3,
                Make = "BMW",
                Model = "3 Series",
                Description = "Premium",
                Price = 80,
                Image = "../images/2023_bmw_3-series_sedan_m340i.jpg"
            });
        }

        public IEnumerable<Vehicle> GetVehicles() => _vehicles;
        public Vehicle? GetVehicle(int id) => _vehicles.FirstOrDefault(v => v.Id == id);
        public void AddOrder(Order order) => _orders.Add(order);
        public Order? GetOrder(string orderId) => _orders.FirstOrDefault(o => o.OrderId == orderId);
        public void UpdateOrder(Order order)
        {
            var idx = _orders.FindIndex(o => o.OrderId == order.OrderId);
            if (idx >= 0) _orders[idx] = order;
        }
    }
}
