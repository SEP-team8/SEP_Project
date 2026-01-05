using webshop_back.Data.Models;

namespace webshop_back.Data.Seed
{
    public static class DbInitializer
    {
        public static void Seed(AppDbContext db)
        {
            if (db.Vehicles.Any()) return;

            byte[] LoadImage(string fileName)
            {
                var basePath = Path.Combine(
                    AppContext.BaseDirectory,
                    "SeedImages"
                );

                var fullPath = Path.Combine(basePath, fileName);

                return File.Exists(fullPath)
                    ? File.ReadAllBytes(fullPath)
                    : null!;
            }

            var vehicles = new[]
            {
                new Vehicle
                {
                    Make = "Toyota",
                    Model = "Corolla",
                    Description = "Compact",
                    Price = 35,
                    Image = LoadImage("toyota-corolla-hybrid.jpg"),
                    MerchantId = "SHOP-123"
                },
                new Vehicle
                {
                    Make = "Skoda",
                    Model = "Octavia",
                    Description = "Family",
                    Price = 45,
                    Image = LoadImage("2024-skoda-octavia-rs.jpg"),
                    MerchantId = "SHOP-123"
                },
                new Vehicle
                {
                    Make = "BMW",
                    Model = "3 Series",
                    Description = "Premium",
                    Price = 80,
                    Image = LoadImage("2023_bmw_3-series_sedan_m340i.jpg"),
                    MerchantId = "SHOP-123"
                }
            };

            db.Vehicles.AddRange(vehicles);
            db.SaveChanges();
        }
    }
}
