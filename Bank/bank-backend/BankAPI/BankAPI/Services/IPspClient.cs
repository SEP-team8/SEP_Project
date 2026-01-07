using BankAPI.DTOs;

namespace BankAPI.Services
{
    public interface IPspClient
    {
        Task NotifyPaymentStatusAsync(PspPaymentStatusDto dto);
    }

}
