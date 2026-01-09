using PSPbackend.Models.Enums;

namespace PSPbackend.DTOs.Bank
{
    public class InitPaymentRequestDto
    {
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public Guid MerchantId { get; set; } // BankMerchantId
        public string Stan { get; set; } = string.Empty;
        public DateTime PspTimestamp { get; set; }

        public InitPaymentRequestDto(decimal amount, Currency currency, Guid bankMerchantId, string stan, DateTime pspTimeStamp)
        {
            Amount = amount;
            Currency = currency;
            MerchantId = bankMerchantId;
            Stan = stan;
            PspTimestamp = pspTimeStamp;
        }

        public InitPaymentRequestDto()
        {
        }
    }
}
