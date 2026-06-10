using PSPbackend.Models;

namespace PSPbackend.Services
{
    public interface IPaymentMethodRouter
    {
        Task<string> RouteAsync(PaymentTransaction transaction, Merchant merchant, CancellationToken ct);
    }
}
