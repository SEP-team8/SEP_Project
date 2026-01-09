using PSPbackend.Models.Enums;

namespace PSPbackend.DTOs
{
    public class PaymentResponseDto
    {
        public List<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
        public Guid MerchantId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Stan { get; set; }
        public DateTime PspTimestamp { get; set; }
    }
}
