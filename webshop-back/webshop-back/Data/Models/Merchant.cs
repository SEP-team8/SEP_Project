using System;
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

        [Required]
        public string ApiKeyHash { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public string? AllowedReturnUrls { get; set; }

        public string? Domain { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApiKeyRotatedAt { get; set; }
        public string? ContactEmail { get; set; }
    }
}
