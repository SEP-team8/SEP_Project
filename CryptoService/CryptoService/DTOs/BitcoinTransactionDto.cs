namespace CryptoService.DTOs;

public sealed class BitcoinTransactionDto
{
    public string TxId { get; set; } = null!;

    public long Fee { get; set; }

    public BitcoinStatusDto Status { get; set; } = null!;
}