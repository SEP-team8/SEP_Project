using System.ComponentModel.DataAnnotations;

namespace BankAPI.Models
{
    public class Card
    {
        public Guid CardId { get; set; }

        // Instead of storing PAN in plaintext, store a stable hash for lookups and the last 4 digits for display.
        [Required]
        [MaxLength(64)] // SHA256 hex length
        public string PanHash { get; set; }

        [Required]
        [MaxLength(4)]
        public string PanLast4 { get; set; }

        public string CardholderName { get; set; }

        [Required]
        [MaxLength(5)] // MM/YY
        public string ExpiryMmYy { get; set; }

        public Guid AccountId { get; set; }
        public BankAccount BankAccount { get; set; }

        // CVV is sensitive; store only the protected value (encrypted) and never log in plain.
        [Required]
        public string EncryptedCvv { get; set; }
    }
}
