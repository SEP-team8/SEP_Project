using PSPbackend.Models.Enums;

namespace PSPbackend.DTOs.Bank
{
    public class BankPaymentStatusDto
    {
        public string Stan { get; set; } = string.Empty;
        public Guid GlobalTransactionId { get; set; }
        public TransactionStatus Status { get; set; }
        public DateTime AcquirerTimestamp { get; set; }
        public Guid MerchantID { get; set; }
        public DateTime PspTimestamp { get; set; }
    }
}
