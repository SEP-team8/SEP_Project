using webshop_back.Data.Models;

namespace webshop_back.DTOs
{
    public class PaymentWebhookRequest
    {
        public Guid MerchantId { get; set; }
        public Guid MerchantOrderId { get; set; } //OrderId!!!
        public OrderStatus? Status { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
    }
}
