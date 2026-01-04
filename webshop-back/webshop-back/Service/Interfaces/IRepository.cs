using webshop_back.Data.Models;

namespace webshop_back.Service.Interfaces
{
    public interface IRepository
    {
        Order? GetOrder(string orderId);
        void AddOrder(Order order);
        void UpdateOrder(Order order);

        IEnumerable<Vehicle> GetVehicles();
        Vehicle? GetVehicle(int id);
        void AddVehicle(Vehicle vehicle);
        void UpdateVehicle(Vehicle vehicle);
        void DeleteVehicle(int id);


        Merchant? GetMerchant(string merchantId);
        Merchant? GetMerchantByMerchantId(string merchantId);
        Merchant? GetMerchantByDomain(string domain);
        void AddMerchant(Merchant merchant);
    }
}
