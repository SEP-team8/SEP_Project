using PSPbackend.DTOs.Bank;
using PSPbackend.Models;

namespace PSPbackend.Services
{
    public interface IBankClient
    {
        Task<InitPaymentResponseDto> InitAsync(InitPaymentRequestDto dto, PaymentMethod paymentMethod, CancellationToken ct);
    }
}
