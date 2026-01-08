namespace PSPbackend.Models
{
    public class Merchant
    {
        public string MerchantId { get; set; } = string.Empty;
        public string MerchantPassword { get; set; } = string.Empty; // MERCHANT_PASSWORD - api-key, čuva se u bazi kao plain tx za sada 
    }
}
