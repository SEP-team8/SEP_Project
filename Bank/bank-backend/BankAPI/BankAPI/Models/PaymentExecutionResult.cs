namespace BankAPI.Models
{
    public enum PaymentExecutionResult
    {
        Success,
        NotFound,
        InvalidState,
        Expired,
        InvalidCard,
        InsufficientFunds
    }

}
