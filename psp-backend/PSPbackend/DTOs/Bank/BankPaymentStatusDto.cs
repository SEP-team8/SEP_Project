using PSPbackend.Models;

namespace PSPbackend.DTOs.Bank
{
    public class BankPaymentStatusDto
    {
        public Guid PaymentRequestId { get; set; }
        public TransactionStatus Status { get; set; }
        public DateTime AcquirerTimestamp { get; set; }
        public string CallbackUrl { get; set; } = string.Empty;
    }
}
