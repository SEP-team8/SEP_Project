using CryptoService.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoService.Persistance;

public class CryptoDbContext : DbContext
{
    public CryptoDbContext(DbContextOptions<CryptoDbContext> options) : base(options) { }
    
    public DbSet<CryptoPayment> CryptoPayments => Set<CryptoPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CryptoPayment>(entity =>
        {
            entity.Property(x => x.BitcoinAddress)
                  .IsRequired()
                  .HasMaxLength(128);

            entity.Property(x => x.BitcoinAmount)
                  .HasPrecision(18, 8);

            entity.Property(x => x.FiatAmount)
                  .HasPrecision(18, 2);

            entity.Property(x => x.Status)
                  .HasConversion<string>();

            entity.Property(x => x.FiatCurrency)
                  .HasConversion<string>();

            entity.HasIndex(x => x.BitcoinAddress)
                  .IsUnique();
        });
    }
}
