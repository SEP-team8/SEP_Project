namespace PSPbackend.DTOs.Bank
{
    public class InitPaymentResponseDto
    {
        public Guid BankPaymentRequestId { get; set; }
        public string PaymentRequestUrl { get; set; } = string.Empty;
        public Guid GlobalTransactionId { get; set; }
    }
}
