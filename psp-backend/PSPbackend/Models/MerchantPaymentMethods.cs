namespace PSPbackend.Models
{
    public class MerchantPaymentMethods
    {
        public Guid MerchantId { get; set; }
        public Merchant Merchant { get; set; }

        public Guid PaymentMethodId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}
