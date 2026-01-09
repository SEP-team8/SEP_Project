using PSPbackend.DTOs.Bank;
using PSPbackend.Models;

namespace PSPbackend.Services
{
    public interface IBankClient
    {
        Task<InitPaymentResponseDto> CreatePaymentAsync(
                PaymentTransaction transaction,
                CancellationToken ct);
    }
}
