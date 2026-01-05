namespace webshop_back.DTOs
{
    public class OrderDto
    {
        public string OrderId { get; set; } = "";
        public string? MerchantId { get; set; }
        public string? PaymentId { get; set; }      // internal PSP payment id
        public string? Stan { get; set; }           // STAN (trace)
        public string? GlobalTransactionId { get; set; } // from PSP callback
        public int? UserId { get; set; }            // nullable, if user logged in
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public string Status { get; set; } = "Initialized";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? PaymentUrl { get; set; } // link to PSP payment page (if applicable)

        public IEnumerable<OrderItemDto>? Items { get; set; }
    }
}
