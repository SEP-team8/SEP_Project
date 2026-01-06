namespace BankAPI.DTOs
{
    public class QRPaymentResponseDto
    {
        public Guid PaymentRequestId { get; set; }
        public string QrCodeBase64 { get; set; } = string.Empty;
    }

}
