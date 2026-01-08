using System.Globalization;

namespace PSPbackend.DTOs
{
    public class PurchaseDto
    {
        public string MerchantOrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
    public class PaymentRequestDto
    {
        public string PaymentMethod { get; set; } = string.Empty; 
        public string MerchantId { get; set; } = string.Empty;     //npr. "shop_001"
        public string MerchantPassword { get; set; } = string.Empty;  //za autentifikaciju
        public DateTime MerchantTimestamp { get; set; }
        public PurchaseDto Purchase { get; set; } = new PurchaseDto();
    }
}
