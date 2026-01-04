namespace BankAPI.Models
{
    public class PaymentRequest
    {
        public Guid PaymentRequestId { get; set; }

        public Guid MerchantId { get; set; }

        public Guid PspId { get; set; }

        public decimal Amount { get; set; }

        public Currency Currency { get; set; }

        public string Stan { get; set; }

        public DateTime PspTimestamp { get; set; }

        public PaymentRequestStatus Status { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
