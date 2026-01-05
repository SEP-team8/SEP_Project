namespace webshop_back.Data.Models
{
    public class Order
    {
        public string OrderId { get; set; } = "";

        public int? UserId { get; set; }
        

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";


        public string? MerchantId { get; set; }

        public string Status { get; set; } = "Initialized";


        public string? PaymentId { get; set; }      // PSP payment id
        public string? PaymentUrl { get; set; }     // redirect url to PSP
        public string? Stan { get; set; }           // optional acquirer stan
        public string? GlobalTransactionId { get; set; } // returned by PSP


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public ICollection<OrderItem>? Items { get; set; }
    }
}
