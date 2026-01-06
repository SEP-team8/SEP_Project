using BankAPI.Models;

namespace BankAPI.DTOs
{
    public class PaymentRequestDto
    {
        public Guid PaymentRequestId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public PaymentRequestStatus Status { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
