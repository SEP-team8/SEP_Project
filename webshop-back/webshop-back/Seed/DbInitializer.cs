using System.Text.Json;
using webshop_back.Data.Models;
using webshop_back.Helpers;

namespace webshop_back.Data.Seed
{
    public static class DbInitializer
    {
        public static void Seed(AppDbContext db)
        {
            SeedMerchants(db);
            SeedVehicles(db);
        }

        // =========================
        // MERCHANT SEED
        // =========================
        private static void SeedMerchants(AppDbContext db)
        {
            if (db.Merchants.Any())
                return;

            var merchants = new List<Merchant>();

            // ---- SHOP-123 ----
            var (rawKey1, hash1) = ApiKeyHasher.Generate();

            merchants.Add(new Merchant
            {
                MerchantId = "SHOP-123",
                Name = "Dev Shop 123",
                Domain = "shop1.localhost",
                IsActive = true,

                ApiKeyHash = hash1,

                PspMerchantId = "PSP-SHOP-123",
                PspMerchantSecret = "psp-secret-123", // kasnije encrypt
                PspEnvironment = "TEST",

                AllowedReturnUrls = JsonSerializer.Serialize(new[]
                {
                    "http://localhost:5173/payment-result"
                }),

                WebhookSecret = "webhook-secret-123",
                ContactEmail = "dev123@shop.local"
            });

            Console.WriteLine($"[SEED] SHOP-123 API KEY (RAW): {rawKey1}");

            // ---- SHOP-321 ----
            var (rawKey2, hash2) = ApiKeyHasher.Generate();

            merchants.Add(new Merchant
            {
                MerchantId = "SHOP-321",
                Name = "Dev Shop 321",
                Domain = "shop2.localhost",
                IsActive = true,

                ApiKeyHash = hash2,

                PspMerchantId = "PSP-SHOP-321",
                PspMerchantSecret = "psp-secret-321",
                PspEnvironment = "TEST",

                AllowedReturnUrls = JsonSerializer.Serialize(new[]
                {
                    "http://localhost:5174/payment-result"
                }),

                WebhookSecret = "webhook-secret-321",
                ContactEmail = "dev321@shop.local"
            });

            Console.WriteLine($"[SEED] SHOP-321 API KEY (RAW): {rawKey2}");

            db.Merchants.AddRange(merchants);
            db.SaveChanges();
        }

        // =========================
        // VEHICLE SEED
        // =========================
        private static void SeedVehicles(AppDbContext db)
        {
            if (db.Vehicles.Any())
                return;

            byte[] LoadImage(string fileName)
            {
                var basePath = Path.Combine(
                    AppContext.BaseDirectory,
                    "SeedImages"
                );

                var fullPath = Path.Combine(basePath, fileName);

                return File.Exists(fullPath)
                    ? File.ReadAllBytes(fullPath)
                    : Array.Empty<byte>();
            }

            var vehicles = new[]
            {
                // SHOP-123
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
                },

                // SHOP-321
                new Vehicle
                {
                    Make = "CCCCCCCCCCCC",
                    Model = "Corolla",
                    Description = "Compact",
                    Price = 35,
                    Image = LoadImage("toyota-corolla-hybrid.jpg"),
                    MerchantId = "SHOP-321"
                },
                new Vehicle
                {
                    Make = "BBBBBBBBBBB",
                    Model = "Octavia",
                    Description = "Family",
                    Price = 45,
                    Image = LoadImage("2024-skoda-octavia-rs.jpg"),
                    MerchantId = "SHOP-321"
                },
                new Vehicle
                {
                    Make = "AAAAAAAAAAA",
                    Model = "3 Series",
                    Description = "Premium",
                    Price = 80,
                    Image = LoadImage("2023_bmw_3-series_sedan_m340i.jpg"),
                    MerchantId = "SHOP-321"
                }
            };

            db.Vehicles.AddRange(vehicles);
            db.SaveChanges();
        }
    }
}
