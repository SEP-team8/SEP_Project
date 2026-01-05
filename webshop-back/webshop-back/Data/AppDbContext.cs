using Microsoft.EntityFrameworkCore;
using webshop_back.Data.Models;

namespace webshop_back.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Merchant> Merchants { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users
            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("Users");
                b.HasKey(u => u.Id);

                b.Property(u => u.Name)
                    .HasMaxLength(255)
                    .IsRequired();

                b.Property(u => u.Email)
                    .HasMaxLength(255)
                    .IsRequired();

                b.HasIndex(u => u.Email)
                    .IsUnique();

                b.Property(u => u.PasswordHash)
                    .HasMaxLength(512)
                    .IsRequired();

                b.Property(u => u.Role)
                    .HasConversion<string>()
                    .HasMaxLength(32)
                    .IsRequired();

                b.Property(u => u.ProfilePicture)
                    .HasColumnType("varbinary(max)")
                    .IsRequired(false);

                b.Property(u => u.MerchantId)
                    .HasMaxLength(100)
                    .IsRequired(false);
            });

            // Vehicles
            modelBuilder.Entity<Vehicle>(b =>
            {
                b.ToTable("Vehicles");
                b.HasKey(v => v.Id);

                b.Property(v => v.Make)
                    .HasMaxLength(128)
                    .IsRequired();

                b.Property(v => v.Model)
                    .HasMaxLength(128)
                    .IsRequired();

                b.Property(v => v.Description)
                    .HasMaxLength(1024);

                b.Property(v => v.Price)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                b.Property(v => v.Image)
                    .HasColumnType("varbinary(max)")
                    .IsRequired(false);

                b.Property(v => v.MerchantId)
                    .HasMaxLength(100)
                    .IsRequired(false);
            });

            // Orders
            modelBuilder.Entity<Order>(b =>
            {
                b.ToTable("Orders");
                b.HasKey(o => o.OrderId);

                b.Property(o => o.OrderId)
                    .HasMaxLength(100)
                    .IsRequired();

                b.Property(o => o.UserId)
                    .IsRequired(false);

                b.Property(o => o.MerchantId)
                    .HasMaxLength(100)
                    .IsRequired(false);

                b.Property(o => o.Amount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                b.Property(o => o.Currency)
                    .HasMaxLength(8)
                    .HasDefaultValue("EUR");

                b.Property(o => o.Status)
                    .HasMaxLength(64)
                    .HasDefaultValue("Initialized");

                b.Property(o => o.PaymentId)
                    .HasMaxLength(200)
                    .IsRequired(false);

                b.Property(o => o.PaymentUrl)
                    .HasColumnType("nvarchar(max)")
                    .IsRequired(false);

                b.Property(o => o.Stan)
                    .HasMaxLength(64)
                    .IsRequired(false);

                b.Property(o => o.GlobalTransactionId)
                    .HasMaxLength(200)
                    .IsRequired(false);

                b.Property(o => o.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                b.Property(o => o.UpdatedAt)
                    .IsRequired(false);

                b.Property(o => o.ExpiresAt)
                    .IsRequired(false);
            });

            // Merchants mapping
            modelBuilder.Entity<Merchant>(b =>
            {
                b.ToTable("Merchants");
                b.HasKey(m => m.Id);

                b.Property(m => m.MerchantId).HasMaxLength(100).IsRequired();
                b.HasIndex(m => m.MerchantId).IsUnique();

                b.Property(m => m.Name).HasMaxLength(255).IsRequired(false);
                b.Property(m => m.ApiKeyHash).HasMaxLength(1024).IsRequired();
                b.Property(m => m.IsActive).HasDefaultValue(true);
                b.Property(m => m.AllowedReturnUrls).HasColumnType("nvarchar(max)").IsRequired(false);
                b.Property(m => m.Domain).HasMaxLength(255).IsRequired(false);
                b.Property(m => m.ContactEmail).HasMaxLength(255).IsRequired(false);
                b.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            //OrderItem mapping
            modelBuilder.Entity<OrderItem>(b =>
            {
                b.ToTable("OrderItems");
                b.HasKey(oi => oi.Id);

                b.Property(oi => oi.OrderId).HasMaxLength(100).IsRequired();
                b.Property(oi => oi.VehicleId).IsRequired();
                b.Property(oi => oi.VehicleName).HasMaxLength(255).IsRequired(false);
                b.Property(oi => oi.PricePerDay).HasColumnType("decimal(18,2)").IsRequired();
                b.Property(oi => oi.Days).IsRequired();

                b.HasOne(oi => oi.Order)
                 .WithMany(o => o.Items)
                 .HasForeignKey(oi => oi.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
