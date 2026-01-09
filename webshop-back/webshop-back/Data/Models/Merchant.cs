using System.ComponentModel.DataAnnotations;

namespace webshop_back.Data.Models
{
    public class Merchant
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string MerchantId { get; set; } = "";

        [MaxLength(255)]
        public string Name { get; set; } = "";
        
        [MaxLength(255)]
        public string? Domain { get; set; }

        public bool IsActive { get; set; } = true;


        // =========================
        // 2) MERCHANT AUTH (WEB → BACKEND)
        // =========================
        [Required]
        public string ApiKeyHash { get; set; } = "";
        public DateTime? ApiKeyRotatedAt { get; set; }


        // =========================
        // 3) PSP CONFIGURATION (BACKEND → PSP)
        // =========================

        [Required]
        [MaxLength(100)]
        public string PspMerchantId { get; set; } = "";

        [Required]
        [MaxLength(512)]
        public string PspMerchantSecret { get; set; } = "";

        [Required]
        [MaxLength(20)]
        public string PspEnvironment { get; set; } = "TEST";


        [MaxLength(512)]
        public string? WebhookSecret { get; set; }

        public string? AllowedReturnUrls { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(255)]
        public string? ContactEmail { get; set; }

        public string? PspConfigSnapshot { get; set; }
    }
}
