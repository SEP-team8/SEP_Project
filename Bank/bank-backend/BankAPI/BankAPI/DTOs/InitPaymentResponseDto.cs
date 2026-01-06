namespace BankAPI.DTOs
{
    public class InitPaymentResponseDto
    {
        public Guid PaymentRequestId { get; set; }
        public string PaymentRequestUrl { get; set; }
    }
}
