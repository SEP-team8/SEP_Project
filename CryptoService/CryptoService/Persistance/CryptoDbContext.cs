using CryptoService.Models;
using CryptoService.Models.WalletModels;
using Microsoft.EntityFrameworkCore;

namespace CryptoService.Persistance
{
    public class CryptoDbContext : DbContext
    {
        public CryptoDbContext(DbContextOptions<CryptoDbContext> options) : base(options) { }

        public DbSet<CryptoPayment> CryptoPayments { get; set; } = null!;
        public DbSet<Wallet> Wallets { get; set; } = null!;
        public DbSet<WalletAddress> WalletAddresses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CryptoPayment>(eb =>
            {
                eb.HasKey(x => x.Id);
                eb.Property(x => x.FiatAmount).HasColumnType("decimal(18,2)");
                eb.Property(x => x.EthAmount).HasColumnType("decimal(36,18)");
                eb.Property(x => x.AmountWei).HasMaxLength(100);
                eb.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<Wallet>(eb =>
            {
                eb.HasKey(x => x.Id);
                eb.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<WalletAddress>(eb =>
            {
                eb.HasKey(x => x.Id);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
