namespace PSPbackend.Models
{
    public class PaymentTransaction
    {

        //transactional id koji je primary key

        // WebShop -> PSP
        public Guid MerchantId { get; set; }
        public Guid MerchantOrderId { get; set; }
        public DateTime MerchantTimestamp { get; set; } 
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }

        // PSP → Bank 
        public string Stan { get; set; } = string.Empty; //generise ga PSP, a banka ga prima i cuva ovoga 6 cifara
        public DateTime PspTimestamp { get; set; } //ovoga

        // Bank → PSP
        public Guid? BankPaymentRequestId { get; set; }
        public DateTime? AcquirerTimestamp { get; set; } 
        public TransactionStatus Status { get; set; }
        public Guid BankMerchantId { get; internal set; } // ovoga
        public Guid GlobalTransactionId { get; set; }  // Generise ga banka a on je identifikator transakcijes

        public PaymentMethod PaymentMethod { get; set; } //odabrani nacin placanja

    }
}
