using webshop_back.DTOs;

namespace webshop_back.Data.Models
{
    public class PaymentInitRequest
    {
        public Guid MerchantId { get; set; }
        public string? MerchantPassword { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public Guid MerchantOrderId { get; set; }               //OrderId
        public DateTime MerchantTimeStamp { get; set; }
        public string SuccessUrl { get; set; } = "";
        public string FailedUrl { get; set; } = "";
        public string ErrorUrl { get; set; } = "";
    }
}
