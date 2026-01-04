namespace BankAPI.DTOs
{
    public enum InitializePaymentResult
    {
        Success,
        InvalidPsp,
        InvalidSignature,
        InvalidMerchant
    }
}
