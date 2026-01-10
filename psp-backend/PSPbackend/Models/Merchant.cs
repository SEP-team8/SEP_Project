namespace PSPbackend.Models
{
    public class Merchant
    {
        public Guid MerchantId { get; set; }
        public Guid BankMerchantId { get; set; }
        public string MerchantPassword { get; set; } = string.Empty;
        public string FailedUrl { get; set; } = string.Empty;
        public string SucessUrl { get; set; } = string.Empty;
        public string ErrorUrl {  get; set; } = string.Empty;

        public ICollection<MerchantPaymentMethods> MerchantPaymentMethods { get; set; }
    }
}
