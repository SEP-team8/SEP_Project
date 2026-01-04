using BankAPI.Models;

namespace BankAPI.DTOs
{
    public class InitPaymentRequestDto
    {
        public Guid MerchantId { get; set; }
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public string Stan { get; set; } = null!;
        public DateTime PspTimestamp { get; set; }
    }
}
