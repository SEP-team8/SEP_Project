using PSPbackend.Models.Enums;

namespace PSPbackend.DTOs
{
    public class SelectPaymentMethodRequestDto
    {
        public Guid MerchantId { get; set; }
        public string Stan { get; set; } = string.Empty;
        public DateTime PspTimestamp { get; set; }
        public PaymentMethodType PaymentMethod { get; set; }
    }

}
