using System.Globalization;

namespace PSPbackend.DTOs
{
    public class PaymentRequestDto
    {
        public Guid MerchantId { get; set; }
        public string MerchantPassword { get; set; } = string.Empty;
        public DateTime MerchantTimestamp { get; set; }
        public Guid MerchantOrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}
