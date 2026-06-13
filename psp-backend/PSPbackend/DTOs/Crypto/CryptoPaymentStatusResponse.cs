namespace PSPbackend.DTOs.Crypto
{
    public class CryptoPaymentStatusResponse
    {
        public Guid PaymentId { get; set; }
        public string Status { get; set; } = "Initialized";

        public string EthAddress { get; set; } = string.Empty;

        public decimal EthAmount { get; set; }

        public int ChainId { get; set; }

        public string? TransactionHash { get; set; }

        public int Confirmations { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }
}
