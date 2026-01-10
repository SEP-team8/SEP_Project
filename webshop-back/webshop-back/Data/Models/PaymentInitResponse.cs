namespace webshop_back.Data.Models
{
    public class PaymentInitResponse
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
    }
}
