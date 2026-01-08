using Microsoft.EntityFrameworkCore;
using PSPbackend.Models;

namespace PSPbackend.Context
{
    public class PspDbContext: DbContext
    {
        public PspDbContext(DbContextOptions<PspDbContext> options) : base(options) { }

        public DbSet<Merchant> Merchants => Set<Merchant>(); //multi tenat
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureMerchant(modelBuilder);
            ConfigurePaymentTransaction(modelBuilder);
        }

        private static void ConfigureMerchant(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Merchant>(entity =>
            {
                entity.HasKey(x => x.MerchantId);

                entity.Property(x => x.MerchantId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.MerchantPassword)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasIndex(x => x.MerchantId)
                    .IsUnique();
            });
        }

        private static void ConfigurePaymentTransaction(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(x => x.TransactionId);

                entity.Property(x => x.MerchantId).IsRequired().HasMaxLength(100);
                entity.Property(x => x.MerchantOrderId).IsRequired().HasMaxLength(120);

                entity.Property(x => x.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                //enum
                entity.Property(x => x.Currency)
                   .HasConversion<int>()
                   .IsRequired();

                entity.Property(x => x.Stan)
                    .IsRequired()
                    .HasMaxLength(32);

                entity.Property(x => x.AcquirerTimestamp)
                    .IsRequired(false);

                entity.Property(x => x.Status)
                    .HasConversion<int>();

                // Po specifikaciji: kombinacija MerchantId + Stan + PspTimestamp može da prati transakciju
                entity.HasIndex(x => new { x.MerchantId, x.Stan, x.PspTimestamp })
                    .IsUnique();

                // Da ti ne duplira isti merchantOrderId za istog merchant-a
                entity.HasIndex(x => new { x.MerchantId, x.MerchantOrderId })
                    .IsUnique();

                // BankPaymentRequestId (ako postoji) da je jedinstven
                entity.HasIndex(x => x.BankPaymentRequestId)
                    .IsUnique();
            });
        }
    }
}
