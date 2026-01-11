using System.ComponentModel.DataAnnotations;

namespace BankAPI.Models
{
    public class Card
    {
        public Guid CardId { get; set; }
        [Required]
        [MaxLength(19)]
        public string PAN { get; set; } // TODO: store this value more safely
        public string CardholderName { get; set; }

        [Required]
        [MaxLength(5)] // MM/YY
        public string ExpiryMmYy { get; set; }

        public Guid AccountId { get; set; }
        public BankAccount BankAccount { get; set; }

        [Required]
        [MaxLength(3)]
        public string Cvv { get; set; } // TODO: store this value more safely

        //[Required]
        //[MaxLength(64)]
        //public string CardToken { get; set; }

        //[Required]
        //[MaxLength(4)]
        //public string PanLast4 { get; set; }
    }
}
