namespace CryptoService.DTOs;

public sealed class BitcoinStatusDto
{
    public bool Confirmed { get; set; }

    public int? BlockHeight { get; set; }

    public string? BlockHash { get; set; }
}
