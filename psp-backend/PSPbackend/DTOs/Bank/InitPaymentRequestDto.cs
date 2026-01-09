using PSPbackend.Models;

namespace PSPbackend.DTOs.Bank
{
    public class InitPaymentRequestDto
    {
        public InitPaymentRequestDto(float amount, Currency currency, Guid merchantId, string stan, DateTime timestamp)
        {
            Amount = amount;
            Currency = currency;
            MerchantId = merchantId;
            Stan = stan;
            Timestamp = timestamp;
        }

        public float Amount { get; set; }
        public Currency Currency {  get; set; }
        public Guid MerchantId { get; set; }
        public string Stan { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
