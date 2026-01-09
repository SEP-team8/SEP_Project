namespace PSPbackend.DTOs
{
    public class StartPaymentResponseDto
    {
        public StartPaymentResponseDto(string paymentRequestUrl)
        {
            PaymentRequestUrl = paymentRequestUrl;
        }
        public string PaymentRequestUrl { get; set; } = string.Empty;
    }
}
