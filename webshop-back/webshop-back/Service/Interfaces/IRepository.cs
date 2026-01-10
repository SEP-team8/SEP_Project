using webshop_back.Data.Models;

namespace webshop_back.Service.Interfaces
{
    public interface IRepository
    {
        Order? GetOrder(Guid orderId);
        void AddOrder(Order order);
        void UpdateOrder(Order order);

        public IEnumerable<Vehicle> GetVehiclesForMerchant(Guid merchantId);
        Vehicle? GetVehicle(int id);
        void AddVehicle(Vehicle vehicle);
        void UpdateVehicle(Vehicle vehicle);
        void DeleteVehicle(int id);

        Merchant? GetMerchantByMerchantId(Guid merchantId);
        Merchant? GetMerchantByDomain(string domain);

        IEnumerable<Order> GetOrdersForUser(int userId);
        Order? GetOrderWithItems(Guid orderId);

    }
}
