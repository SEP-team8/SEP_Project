using System.ComponentModel.DataAnnotations;

namespace webshop_back.Data.Models
{
    public class Merchant
    {
        [Required]
        public Guid MerchantId { get; set; }

        [MaxLength(255)]
        public string Name { get; set; } = "";
        
        [MaxLength(255)]
        public string? Domain { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public string ApiKeyHash { get; set; } = "";

        [Required]
        [MaxLength(100)]
        public Guid PspMerchantId { get; set; }

        [Required]
        [MaxLength(512)]
        public string PspMerchantSecret { get; set; } = "";

        [MaxLength(512)]
        public string? WebhookSecret { get; set; }

        public string? AllowedReturnUrls { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(255)]
        public string? ContactEmail { get; set; }
    }
}
