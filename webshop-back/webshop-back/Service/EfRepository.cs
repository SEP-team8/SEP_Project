using webshop_back.Data;
using webshop_back.Data.Models;
using webshop_back.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace webshop_back.Service
{
    public class EfRepository : IRepository
    {
        private readonly AppDbContext _db;

        public EfRepository(AppDbContext db)
        {
            _db = db;
        }

        public Order? GetOrder(Guid orderId)
        {
            return _db.Orders.AsNoTracking().FirstOrDefault(o => o.OrderId == orderId);
        }

        public void AddOrder(Order order)
        {
            _db.Orders.Add(order);
            _db.SaveChanges();
        }

        public void UpdateOrder(Order order)
        {
            var existing = _db.Orders.Find(order.OrderId);
            if (existing == null) return;
            _db.Entry(existing).CurrentValues.SetValues(order);
            _db.SaveChanges();
        }

        public IEnumerable<Vehicle> GetVehiclesForMerchant(Guid merchantId)
        {
            return _db.Vehicles
                .AsNoTracking()
                .Where(v => v.MerchantId == merchantId)
                .ToList();
        }

        public Vehicle? GetVehicle(int id)
        {
            return _db.Vehicles.AsNoTracking().FirstOrDefault(v => v.Id == id);
        }

        public void AddVehicle(Vehicle vehicle)
        {
            _db.Vehicles.Add(vehicle);
            _db.SaveChanges();
        }

        public void UpdateVehicle(Vehicle vehicle)
        {
            var existing = _db.Vehicles.Find(vehicle.Id);
            if (existing == null) return;

            existing.Make = vehicle.Make;
            existing.Model = vehicle.Model;
            existing.Description = vehicle.Description;
            existing.Price = vehicle.Price;
            existing.Image = vehicle.Image;

            _db.SaveChanges();
        }

        public void DeleteVehicle(int id)
        {
            var existing = _db.Vehicles.Find(id);
            if (existing == null) return;

            _db.Vehicles.Remove(existing);
            _db.SaveChanges();
        }

        public Merchant? GetMerchantByMerchantId(Guid merchantId)
        {
            if (merchantId == Guid.Empty)
                return null;

            return _db.Set<Merchant>()
                .AsNoTracking()
                .FirstOrDefault(m => m.MerchantId == merchantId && m.IsActive);
        }


        public Merchant? GetMerchantByDomain(string domain)
        {
            if (string.IsNullOrEmpty(domain)) return null;

            var host = domain.Trim().ToLowerInvariant();

            var exact = _db.Set<Merchant>().AsNoTracking()
                .FirstOrDefault(m => !string.IsNullOrEmpty(m.Domain) && m.Domain.ToLower() == host && m.IsActive);
            if (exact != null) return exact;

            var allWithDomain = _db.Set<Merchant>().AsNoTracking()
                .Where(m => !string.IsNullOrEmpty(m.Domain) && m.IsActive)
                .ToList();

            foreach (var m in allWithDomain)
            {
                var md = m.Domain!.Trim().ToLowerInvariant();
                if (host == md) return m;
                if (host.EndsWith("." + md)) return m;
            }

            return null;
        }

        public IEnumerable<Order> GetOrdersForUser(int userId)
        {
            return _db.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();
        }

        public Order? GetOrderWithItems(Guid orderId)
        {
            return _db.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .FirstOrDefault(o => o.OrderId == orderId);
        }
    }
}
