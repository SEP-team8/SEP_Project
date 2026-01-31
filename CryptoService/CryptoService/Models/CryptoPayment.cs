namespace CryptoService.Models;

public sealed class CryptoPayment
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public decimal FiatAmount { get; set; }

    public Currency FiatCurrency { get; set; }

    public decimal BitcoinAmount { get; set; }

    public required string BitcoinAddress { get; set; } // address of the receiver (webshop)

    public CryptoPaymentStatus Status { get; set; }

    public string? TransactionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }
}