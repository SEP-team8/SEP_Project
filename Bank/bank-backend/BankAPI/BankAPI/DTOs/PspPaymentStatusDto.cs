using BankAPI.Models;

namespace BankAPI.DTOs
{
    public class PspPaymentStatusDto
    {
        public Guid PaymentRequestId { get; set; }
        public string Stan { get; set; }
        public Guid? GlobalTransactionId { get; set; }
        public DateTime AcquirerTimestamp { get; set; }
        public TransactionStatus Status { get; set; }
        public Guid MerchantID { get; set; }
        public DateTime PspTimestamp { get; set; }
    }

}
