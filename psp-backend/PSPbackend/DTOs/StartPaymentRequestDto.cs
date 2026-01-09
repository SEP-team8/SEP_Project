using PSPbackend.Models;

namespace PSPbackend.DTOs
{
    public class StartPaymentRequestDto
    {
        public Guid MerchantId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}
