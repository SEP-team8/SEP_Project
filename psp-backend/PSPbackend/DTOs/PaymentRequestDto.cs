namespace PSPbackend.DTOs
{
    public class PurchaseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Merchant { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
    public class PaymentRequestDto
    {
        public string PaymentMethod { get; set; } = string.Empty; // card, qr, paypal, crypto..
        public PurchaseDto Purchase { get; set; } = new PurchaseDto();
    }
}
