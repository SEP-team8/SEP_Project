using PSPbackend.Models.Enums;

namespace PSPbackend.DTOs
{
    public class PaymentResponseDto
    {
        public List<PaymentMethodType> PaymentMethods { get; set; } = new List<PaymentMethodType>();
        public Guid MerchantId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Stan { get; set; }
        public DateTime PspTimestamp { get; set; }
    }
}
