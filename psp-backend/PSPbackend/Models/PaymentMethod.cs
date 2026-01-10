using PSPbackend.Models.Enums;

namespace PSPbackend.Models
{
    public class PaymentMethod
    {
        public Guid PaymentMethodId { get; set; }

        public PaymentMethodType PaymentMethodType { get; set; }
    }
}
