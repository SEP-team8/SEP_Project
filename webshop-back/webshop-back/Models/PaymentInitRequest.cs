namespace webshop_back.Models
{
    public class PaymentInitRequest
    {
        public string MERCHANT_ID { get; set; } = "";
        public string? MERCHANT_PASSWORD { get; set; }
        public decimal AMOUNT { get; set; }
        public string CURRENCY { get; set; } = "EUR";
        public string MERCHANT_ORDER_ID { get; set; } = "";
        public string MERCHANT_TIMESTAMP { get; set; } = "";
        public string SUCCESS_URL { get; set; } = "";
        public string FAILED_URL { get; set; } = "";
        public string ERROR_URL { get; set; } = "";
        public string? Method { get; set; } // "card" or "qr"
    }
}
