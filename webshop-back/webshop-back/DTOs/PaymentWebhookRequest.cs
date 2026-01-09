using webshop_back.Data.Models;

namespace webshop_back.DTOs
{
    public class PaymentWebhookRequest
    {
        public string? MerchantId { get; set; }
        public string? MerchantOrderId { get; set; } //OrderId!!!
        public string? PaymentId { get; set; }
        public string? Stan { get; set; }
        public string? GlobalTransactionId { get; set; }
        public OrderStatus? Status { get; set; } // SUCCESS / FAILED / ERROR / EXPIRED
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
    }
}
