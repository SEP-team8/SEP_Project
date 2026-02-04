using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoService.Models
{
    public sealed class CryptoPayment
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public decimal FiatAmount { get; set; }
        public Currency FiatCurrency { get; set; }

        [Column(TypeName = "decimal(36,18)")]
        public decimal EthAmount { get; set; }
        public string AmountWei { get; set; } = "0";
        public string EthAddress { get; set; } = string.Empty;
        public CryptoPaymentStatus Status { get; set; }
        public string? TransactionHash { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        // correlation to PSP
        public Guid? MerchantId { get; set; }    // PSP merchantId (psp side)
        public string? Stan { get; set; }        // PSP stan
        public DateTime? PspTimestamp { get; set; }
    }

}
