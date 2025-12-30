using System.Text.Json;
using webshop_back.Models;

namespace webshop_back.Service
{
    public class DataStore
    {
        private readonly string _filePath;
        private readonly object _lock = new object();

        public DataStore(IConfiguration config)
        {
            var baseDir = AppContext.BaseDirectory;
            _filePath = Path.Combine(baseDir, "..", "data.json");
            // If the path above doesn't fit your build output, override in appsettings or set path absolute.
            // We'll fallback: try current dir first
            if (!File.Exists(_filePath))
            {
                var alt = Path.Combine(Environment.CurrentDirectory, "data.json");
                if (File.Exists(alt)) _filePath = alt;
            }
            // Ensure file exists
            if (!File.Exists(_filePath))
            {
                var initial = new
                {
                    users = new List<object>(),
                    vehicles = new List<object>(),
                    orders = new List<object>(),
                    payments = new List<object>()
                };
                File.WriteAllText(_filePath, JsonSerializer.Serialize(initial, new JsonSerializerOptions { WriteIndented = true }));
            }
        }

        private T Read<T>()
        {
            lock (_lock)
            {
                var txt = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<T>(txt, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            }
        }

        private void Write<T>(T obj)
        {
            lock (_lock)
            {
                File.WriteAllText(_filePath, JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));
            }
        }

        public List<Vehicle> GetVehicles()
        {
            var root = Read<JsonElement>();
            if (root.TryGetProperty("vehicles", out var vArr))
            {
                var vehicles = JsonSerializer.Deserialize<List<Vehicle>>(vArr.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                return vehicles;
            }
            return new List<Vehicle>();
        }

        public List<User> GetUsers()
        {
            var root = Read<JsonElement>();
            var users = root.GetProperty("users");
            return JsonSerializer.Deserialize<List<User>>(users.GetRawText()) ?? new List<User>();
        }

        public List<Order> GetOrders()
        {
            var root = Read<JsonElement>();
            var arr = root.GetProperty("orders");
            return JsonSerializer.Deserialize<List<Order>>(arr.GetRawText()) ?? new List<Order>();
        }

        public List<Payment> GetPayments()
        {
            var root = Read<JsonElement>();
            var arr = root.GetProperty("payments");
            return JsonSerializer.Deserialize<List<Payment>>(arr.GetRawText()) ?? new List<Payment>();
        }

        public void SaveAll(List<User> users, List<Vehicle> vehicles, List<Order> orders, List<Payment> payments)
        {
            var root = new
            {
                users,
                vehicles,
                orders,
                payments
            };
            Write(root);
        }

        // convenience wrappers
        public void AddUser(User u)
        {
            var users = GetUsers();
            users.Add(u);
            var vehicles = GetVehicles();
            var orders = GetOrders();
            var payments = GetPayments();
            SaveAll(users, vehicles, orders, payments);
        }

        public void AddOrder(Order o)
        {
            var users = GetUsers();
            var vehicles = GetVehicles();
            var orders = GetOrders();
            orders.Add(o);
            var payments = GetPayments();
            SaveAll(users, vehicles, orders, payments);
        }

        public void AddPayment(Payment p)
        {
            var users = GetUsers();
            var vehicles = GetVehicles();
            var orders = GetOrders();
            var payments = GetPayments();
            payments.Add(p);
            SaveAll(users, vehicles, orders, payments);
        }

        public void UpdateOrder(Order o)
        {
            var users = GetUsers();
            var vehicles = GetVehicles();
            var orders = GetOrders();
            var payments = GetPayments();
            var idx = orders.FindIndex(x => x.Id == o.Id);
            if (idx >= 0) orders[idx] = o;
            SaveAll(users, vehicles, orders, payments);
        }

        public void UpdatePayment(Payment p)
        {
            var users = GetUsers();
            var vehicles = GetVehicles();
            var orders = GetOrders();
            var payments = GetPayments();
            var idx = payments.FindIndex(x => x.Id == p.Id);
            if (idx >= 0) payments[idx] = p;
            SaveAll(users, vehicles, orders, payments);
        }
    }
}
