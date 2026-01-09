namespace webshop_back.Data.Models
{
    public enum OrderStatus
    {
        Initialized,
        Pending,
        Authorized,
        Success,
        Failed,
        Expired,
        Cancelled,
        Refunded
    }
}
