using PSPbackend.Models.Enums;

namespace PSPbackend.DTOs.Crypto
{
    public class CryptoPaymentNotificationDto
    {
        // PSP identifiers (forwarded from PSP when creating crypto payment)
        public Guid MerchantID { get; set; }            // bankMerchantId as known by PSP
        public string Stan { get; set; } = string.Empty;
        public DateTime PspTimestamp { get; set; }

        // Status information (re-use TransactionStatus enum)
        public TransactionStatus Status { get; set; }

        // Optional: tx hash on-chain
        public string? TransactionHash { get; set; }

        // Optional: id of crypto payment in CryptoService
        public Guid? CryptoPaymentId { get; set; }

        // Optional: global transaction id (if CryptoService creates one)
        public Guid? GlobalTransactionId { get; set; }

        // Timestamp from acquirer side (usually same as when CryptoService detected)
        public DateTime AcquirerTimestamp { get; set; }
    }
}
