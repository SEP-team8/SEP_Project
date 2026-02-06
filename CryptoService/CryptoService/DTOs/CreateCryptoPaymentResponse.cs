namespace CryptoService.DTOs;

public class CreateCryptoPaymentResponse
{
    public Guid PaymentId { get; set; }
    public string EthAddress { get; set; } = string.Empty;
    public decimal EthAmount { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int ChainId { get; set; }
}

