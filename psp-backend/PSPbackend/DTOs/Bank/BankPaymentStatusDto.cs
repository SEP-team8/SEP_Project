using PSPbackend.Models;

namespace PSPbackend.DTOs.Bank
{
    public class BankPaymentStatusDto
    {
        public string Stan { get; set; } = string.Empty;
        public Guid GlobalTransactionId { get; set; }
        public TransactionStatus Status { get; set; }
        public DateTime AcquirerTimestamp { get; set; }
    }
}
