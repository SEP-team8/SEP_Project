using PSPbackend.Models;

namespace PSPbackend.Services
{
    public interface IPayPalServiceClient
    {
        Task<string> CreateOrderAsync(PaymentTransaction transaction, string returnUrl, string cancelUrl, CancellationToken ct);
        Task CaptureOrderAsync(string paypalOrderId, CancellationToken ct);
    }
}
