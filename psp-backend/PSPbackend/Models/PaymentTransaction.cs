namespace PSPbackend.Models
{
    public class PaymentTransaction
    {
        public Guid TransactionId { get; set; } = Guid.NewGuid(); // PSP transactionId primary key

        // WebShop -> PSP
        public string MerchantId { get; set; } = null!;
        public string MerchantOrderId { get; set; } = null!;
        public DateTime MerchantTimestamp { get; set; } //vreme kada se inicijalizuje transakcija, generise ga webshop 

        public decimal Amount { get; set; }
        public Currency Currency { get; set; }

        // PSP → Bank 
        public string Stan { get; set; } = null!;
        public DateTime PspTimestamp { get; set; }
        public Guid BankMerchantId { get; set; } // merchant id kod banke (iz appsettings)

        // Bank → PSP
        public Guid? BankPaymentRequestId { get; set; } // PaymentRequestId iz banke
        public DateTime? AcquirerTimestamp { get; set; }  // Vreme kada banka potvrdjuje transakciju

        public TransactionStatus Status { get; set; } = TransactionStatus.Created;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow; //generise psp, trenutak kada se upisuje u bazu transakcija
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
