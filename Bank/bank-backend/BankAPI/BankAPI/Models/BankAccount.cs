namespace BankAPI.Models
{
    public class BankAccount
    {
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public Currency Currency { get; set; }
    }
}
