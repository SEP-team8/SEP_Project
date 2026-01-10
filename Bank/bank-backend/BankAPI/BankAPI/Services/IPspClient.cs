using BankAPI.DTOs;

namespace BankAPI.Services
{
    public interface IPspClient
    {
        Task<string> NotifyPaymentStatusAsync(PspPaymentStatusDto dto);
    }

}
