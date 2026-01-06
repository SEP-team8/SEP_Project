namespace BankAPI.DTOs
{
    public class IpsQrData
    {
        public string Currency { get; set; } = "RSD";
        public decimal Amount { get; set; }

        public string MerchantAccount { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;

        public string Purpose { get; set; } = "Placanje";
        public string PaymentCode { get; set; } = "289"; // standard code for payments
    }
}
