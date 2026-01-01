using webshop_back.Models;

namespace webshop_back.Service
{
    public interface IRepository
    {
        IEnumerable<Vehicle> GetVehicles();
        Vehicle? GetVehicle(int id);
        void AddOrder(Order order);
        Order? GetOrder(string orderId);
        void UpdateOrder(Order order);
    }
}
