using webshop_back.DTOs;

namespace webshop_back.Data.Models
{
    public class PaymentInitRequest
    {
        public string MerchantId { get; set; } = "";
        public string? MerchantPassword { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public string MerchantOrderId { get; set; } = ""; //OrderId
        public string MerchantTimeStamp { get; set; } = "";
        public string SuccessUrl { get; set; } = "";
        public string FailedUrl { get; set; } = "";
        public string ErrorUrl { get; set; } = "";

        public List<OrderItemDto>? Items { get; set; }
    }
}
