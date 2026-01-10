namespace PSPbackend.DTOs
{
    public class AddMerchantPaymentMethodDto
    {
        public Guid MerchantId { get; set; }
        public string PaymentMethodType { get; set; } = string.Empty;
    }
}
