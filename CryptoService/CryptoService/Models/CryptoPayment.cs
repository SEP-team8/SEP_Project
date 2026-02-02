namespace CryptoService.Models;

public sealed class CryptoPayment
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public decimal FiatAmount { get; set; }

    public Currency FiatCurrency { get; set; }

    // ETH decimal amount (for display)
    public decimal EthAmount { get; set; }

    // store wei as string to avoid precision loss
    public string AmountWei { get; set; } = "0";

    // receiving ETH address for this payment
    public required string EthAddress { get; set; }

    public CryptoPaymentStatus Status { get; set; }

    // transaction hash if known
    public string? TransactionHash { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }
}
