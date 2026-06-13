namespace PSPbackend.DTOs.Crypto
{
    public class CreateCryptoPaymentRequest
    {
        public Guid MerchantId { get; set; }
        public decimal FiatAmount { get; set; }
        public int Currency { get; set; }
        public string Stan { get; set; } = string.Empty;
        public DateTime PspTimestamp { get; set; }
    }
}
