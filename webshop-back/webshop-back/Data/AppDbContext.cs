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

                // ENUM -> STRING
                b.Property(u => u.Role)
                    .HasConversion<string>()
                    .HasMaxLength(32)
                    .IsRequired();

                // profile picture
                b.Property(u => u.ProfilePicture)
                    .HasColumnType("varbinary(max)")
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
                    .IsRequired();

                b.Property(o => o.Amount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                b.Property(o => o.Currency)
                    .HasMaxLength(8)
                    .HasDefaultValue("EUR");

                b.Property(o => o.Status)
                    .HasMaxLength(64)
                    .HasDefaultValue("Initialized");

                b.Property(o => o.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
