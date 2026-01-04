namespace BankAPI.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }

        public Guid PaymentRequestId { get; set; }

        public Guid GlobalTransactionId { get; set; }

        public DateTime AcquirerTimestamp { get; set; }

        public TransactionStatus Status { get; set; }
    }
}
