namespace PSPbackend.DTOs.Bank
{
    public class InitPaymentResponseDto
    {
        public Guid PaymentRequestId { get; set; }
        public string PaymentRequestUrl { get; set; } = string.Empty;
    }
}
