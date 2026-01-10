using PSPbackend.Models.Enums;

namespace PSPbackend.DTOs
{
    public class StartPaymentRequestDto
    {
        public Guid MerchantId { get; set; }
        public PaymentMethodType PaymentMethod { get; set; }
    }
}
