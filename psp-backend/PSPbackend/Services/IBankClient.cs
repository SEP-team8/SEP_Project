using PSPbackend.DTOs.Bank;

namespace PSPbackend.Services
{
    public interface IBankClient
    {
        Task<InitPaymentResponseDto> InitAsync(InitPaymentRequestDto dto, bool isQrPayment, CancellationToken ct);
    }
}
