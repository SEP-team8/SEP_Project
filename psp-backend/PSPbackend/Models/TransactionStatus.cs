namespace PSPbackend.Models
{
    public enum TransactionStatus
    {
        Created = 0,
        RedirectedToBank = 1,
        Success = 2,
        Failed = 3,
        Error = 4
    }
}
