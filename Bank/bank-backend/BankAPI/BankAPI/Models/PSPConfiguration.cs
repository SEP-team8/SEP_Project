namespace BankAPI.Models
{
    public class PSPConfiguration
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string HMACKey { get; set; } // TODO encrypt this
    }
}
