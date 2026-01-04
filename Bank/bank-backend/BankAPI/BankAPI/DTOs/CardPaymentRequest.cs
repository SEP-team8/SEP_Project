namespace BankAPI.DTOs
{
    public class CardPaymentRequest
    {
        public string CardNumber { get; set; }
        public string Expiry { get; set; } // MM/YY
        public string Cvv { get; set; }
        public string CardHolder { get; set; }
    }
}
