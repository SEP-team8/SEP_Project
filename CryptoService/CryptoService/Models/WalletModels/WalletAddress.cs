namespace CryptoService.Models.WalletModels;

public sealed class WalletAddress
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;
    public string Address { get; set; } = string.Empty;
    public string? PublicKey { get; set; }
    public string? PrivateKeyEncrypted { get; set; } // Encrypted private key
    public int DerivationIndex { get; set; }
    public string DerivationPath { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Balance { get; set; }
}
