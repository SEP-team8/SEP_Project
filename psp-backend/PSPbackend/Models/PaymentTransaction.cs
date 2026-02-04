using PSPbackend.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSPbackend.Models
{
    public class PaymentTransaction
    {
        public Guid MerchantId { get; set; }
        public Merchant Merchant { get; set; }
        public Guid MerchantOrderId { get; set; }
        public DateTime MerchantTimestamp { get; set; } 
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }

        public string Stan { get; set; } = string.Empty; //generise ga PSP, a banka ga prima i cuva ovoga 6 cifara
        public DateTime PspTimestamp { get; set; }
        public PaymentMethodType PaymentMethod { get; set; }


        public DateTime? AcquirerTimestamp { get; set; } 
        public TransactionStatus Status { get; set; }
        public Guid GlobalTransactionId { get; set; }

        public Guid? CryptoPaymentId { get; set; }
        public string? CryptoAddress { get; set; }
        public string? TransactionHash { get; set; }

        [Column(TypeName = "decimal(36,18)")]
        public decimal? CryptoAmount { get; set; }
    }
}
