namespace CryptoService.Models;

public enum CryptoPaymentStatus
{
    Pending,
    Detected,       // tx seen but unconfirmed
    Confirmed,
    Expired,
    Failed
}
