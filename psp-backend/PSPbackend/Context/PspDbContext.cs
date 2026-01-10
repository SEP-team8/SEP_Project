using Microsoft.EntityFrameworkCore;
using PSPbackend.Models;

namespace PSPbackend.Context
{
    public class PspDbContext: DbContext
    {
        public PspDbContext(DbContextOptions<PspDbContext> options) : base(options) { }

        public DbSet<Merchant> Merchants => Set<Merchant>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
        public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
        public DbSet<MerchantPaymentMethods> MerchantPaymentMethods => Set<MerchantPaymentMethods>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureMerchant(modelBuilder);
            ConfigurePaymentMethod(modelBuilder);
            ConfigureMerchantPaymentMethods(modelBuilder);
            ConfigurePaymentTransaction(modelBuilder);
        }

        private static void ConfigureMerchant(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Merchant>(entity =>
            {
                entity.ToTable("Merchants");

                entity.HasKey(e => e.MerchantId);

                entity.Property(e => e.MerchantId)
                      .ValueGeneratedNever();

                entity.Property(e => e.BankMerchantId)
                      .IsRequired();

                entity.Property(e => e.MerchantPassword)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(e => e.FailedUrl)
                      .IsRequired()
                      .HasMaxLength(2048);

                entity.Property(e => e.SucessUrl)
                      .IsRequired()
                      .HasMaxLength(2048);

                entity.Property(e => e.ErrorUrl)
                      .IsRequired()
                      .HasMaxLength(2048);
            });
        }

        private static void ConfigureMerchantPaymentMethods(ModelBuilder modelBuilder) 
        {
            modelBuilder.Entity<MerchantPaymentMethods>(entity =>
            {
                entity.ToTable("MerchantPaymentMethods");

                entity.HasKey(e => new
                {
                    e.MerchantId,
                    e.PaymentMethodId
                });

                entity.HasOne(e => e.Merchant)
                  .WithMany(m => m.MerchantPaymentMethods)
                  .HasForeignKey(e => e.MerchantId)
                  .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.PaymentMethod)
                      .WithMany()
                      .HasForeignKey(e => e.PaymentMethodId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigurePaymentMethod(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.ToTable("PaymentMethods");

                entity.HasKey(e => e.PaymentMethodId);

                entity.Property(e => e.PaymentMethodId)
                      .ValueGeneratedNever();

                entity.Property(e => e.PaymentMethodType)
                      .IsRequired()
                      .HasConversion<int>();

                entity.HasIndex(e => e.PaymentMethodType)
                      .IsUnique();
            });
        }


        private static void ConfigurePaymentTransaction(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.ToTable("PaymentTransactions");

                // --- COMPOSITE PRIMARY KEY ---
                entity.HasKey(e => new
                {
                    e.MerchantId,
                    e.Stan,
                    e.PspTimestamp
                });

                // --- Merchant relationship ---
                entity.HasOne(e => e.Merchant)
                      .WithMany()
                      .HasForeignKey(e => e.MerchantId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.MerchantOrderId)
                      .IsRequired();

                entity.Property(e => e.MerchantTimestamp)
                      .IsRequired();

                entity.Property(e => e.Amount)
                      .IsRequired()
                      .HasPrecision(18, 2);

                entity.Property(e => e.Currency)
                      .IsRequired()
                      .HasConversion<int>();

                entity.Property(e => e.Stan)
                      .IsRequired()
                      .HasMaxLength(6)
                      .IsFixedLength();

                entity.Property(e => e.PspTimestamp)
                      .IsRequired();

                entity.Property(e => e.PaymentMethod)
                      .IsRequired()
                      .HasConversion<int>();

                entity.Property(e => e.AcquirerTimestamp)
                      .IsRequired(false);

                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasConversion<int>();

                entity.Property(e => e.GlobalTransactionId)
                      .IsRequired();
            });
        }
    }
}
