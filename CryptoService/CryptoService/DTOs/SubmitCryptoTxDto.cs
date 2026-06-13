namespace CryptoService.DTOs
{
    public class SubmitCryptoTxDto
    {
        public Guid PaymentId { get; set; }
        public string TxHash { get; set; } = string.Empty;
        public string? FromAddress { get; set; }
    }
}
