namespace CryptoService.DTOs.AddressTransaction;

/// <summary>
/// Transaction output (vout) returned by the blockhain API.
/// </summary>
public sealed class VoutDto
{
    public long Value { get; set; } // satoshis

    public string ScriptPubKey { get; set; } = null!;
}
