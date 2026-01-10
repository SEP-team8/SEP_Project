using webshop_back.Data.Models;

namespace webshop_back.DTOs
{
    public class CallbackDto
    {
        public Guid OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Amount { get; set; }
        public Guid MerchantId { get; set; }
    }
}
