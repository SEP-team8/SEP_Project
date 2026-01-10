namespace PSPbackend.DTOs
{
    public class MerchantMethodRowDto
    {
        public Guid MerchantId { get; set; }
        public string PaymentMethodType { get; set; } = string.Empty;
    }
}
