using BankAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BankAPI.Context
{
    public class BankingDbContext: DbContext
    {
        public BankingDbContext(DbContextOptions<BankingDbContext> options)
        : base(options)
        {
        }

        public DbSet<PSPConfiguration> Psps => Set<PSPConfiguration>();
        public DbSet<Merchant> Merchants => Set<Merchant>();
        public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
        public DbSet<Card> Cards => Set<Card>();
        public DbSet<PaymentRequest> PaymentRequests => Set<PaymentRequest>();
        public DbSet<Transaction> Transactions => Set<Transaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigurePsp(modelBuilder);      
            ConfigureBankAccount(modelBuilder);
            ConfigureMerchant(modelBuilder);
            ConfigureCard(modelBuilder);
            ConfigurePaymentRequest(modelBuilder);
            ConfigureTransaction(modelBuilder);
        }

        private static void ConfigurePsp(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PSPConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.HMACKey)
                    .IsRequired()
                    .HasMaxLength(255);
            });
        }

        private static void ConfigureMerchant(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Merchant>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.AccountId)
                    .IsRequired();

                entity.HasOne(e => e.BankAccount)
                    .WithOne()
                    .HasForeignKey<Merchant>(e => e.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.AccountId)
                    .IsUnique();
            });
        }

        private static void ConfigureBankAccount(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.HasKey(e => e.AccountId);

                entity.Property(e => e.Balance)
                    .HasPrecision(18,2)
                    .IsRequired();

                entity.Property(e => e.Currency)
                    .HasConversion<int>()
                    .IsRequired();
            });
        }

        private static void ConfigureCard(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Card>(entity =>
            {
                entity.HasKey(e => e.CardId);

                entity.Property(e => e.PAN)
                    .IsRequired();

                //entity.Property(e => e.CardToken)
                //    .IsRequired()
                //    .HasMaxLength(64);

                //entity.Property(e => e.PanLast4)
                //    .IsRequired()
                //    .HasMaxLength(4);

                entity.Property(e => e.CardholderName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ExpiryMmYy)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.Property(e => e.AccountId)
                    .IsRequired();

                entity.HasOne(e => e.BankAccount)
                    .WithOne()
                    .HasForeignKey<Card>(e => e.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.PAN)
                    .IsUnique();
            });
        }

        private static void ConfigurePaymentRequest(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentRequest>(entity =>
            {
                entity.HasKey(e => e.PaymentRequestId);

                entity.Property(e => e.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(e => e.Currency)
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(e => e.Stan)
                    .IsRequired();

                entity.Property(e => e.PspTimestamp)
                    .IsRequired();

                entity.Property(e => e.ExpiresAt)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasConversion<int>()
                    .IsRequired();

                entity.HasIndex(e => new { e.MerchantId, e.Stan, e.PspTimestamp })
                    .IsUnique();

                entity.HasOne<PSPConfiguration>()
                    .WithMany()
                    .HasForeignKey(e => e.PspId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Merchant)
                    .WithMany()
                    .HasForeignKey(e => e.MerchantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureTransaction(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.TransactionId);

                entity.Property(e => e.GlobalTransactionId)
                    .IsRequired();

                entity.Property(e => e.AcquirerTimestamp)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasConversion<int>()
                    .IsRequired();

                entity.HasOne<PaymentRequest>()
                    .WithMany()
                    .HasForeignKey(e => e.PaymentRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.GlobalTransactionId)
                    .IsUnique();
            });
        }
    }
}
