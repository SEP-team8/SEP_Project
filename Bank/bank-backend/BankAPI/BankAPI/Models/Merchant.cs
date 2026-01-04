namespace BankAPI.Models
{
    public class Merchant
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid AccountId { get; set; }
        public BankAccount BankAccount { get; set; }
    }
}
