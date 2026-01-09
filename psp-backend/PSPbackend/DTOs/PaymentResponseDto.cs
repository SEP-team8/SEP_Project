namespace PSPbackend.DTOs
{
    public class PaymentResponseDto
    {
        public PaymentResponseDto(Guid TransactionId, Guid PaymentRequestId, string RedirectUrl)
        {
            this.TransactionId = TransactionId;
            this.PaymentRequestId = PaymentRequestId;
            this.RedirectUrl = RedirectUrl;
        }

        public string RedirectUrl { get; set; } = string.Empty;
        public Guid TransactionId { get; set; }
        public Guid PaymentRequestId {  get; set; }
    }
}
