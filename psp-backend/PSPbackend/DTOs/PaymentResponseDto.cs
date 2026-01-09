using PSPbackend.Models;

namespace PSPbackend.DTOs
{
    public class PaymentResponseDto
    {
        public List<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
        public Guid MerchantId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}
