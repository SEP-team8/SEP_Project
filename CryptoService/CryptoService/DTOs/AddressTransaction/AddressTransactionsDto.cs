namespace CryptoService.DTOs.AddressTransaction;

public sealed class AddressTransactionsDto
{
    public string TxId { get; set; } = null!;

    public BitcoinStatusDto Status { get; set; } = null!;

    public List<VoutDto> Vout { get; set; } = new();
}
