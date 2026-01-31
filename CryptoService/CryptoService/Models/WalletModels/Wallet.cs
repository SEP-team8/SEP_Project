namespace CryptoService.Models.WalletModels;

public sealed class Wallet
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Mnemonic { get; set; } // Encrypted
    public string? EncryptedSeed { get; set; }
    public string? PublicMasterKey { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int CurrentAddressIndex { get; set; } = 0;

    public ICollection<WalletAddress> Addresses { get; set; } = new List<WalletAddress>();
}
