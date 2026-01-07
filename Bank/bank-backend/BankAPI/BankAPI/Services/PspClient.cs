using BankAPI.DTOs;

namespace BankAPI.Services
{
    public class PspClient : IPspClient
    {
        private readonly HttpClient _httpClient;

        public PspClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task NotifyPaymentStatusAsync(PspPaymentStatusDto dto)
        {
            // Change this
            await _httpClient.PostAsJsonAsync(
                "/api/psp/payments/status",
                dto
            );
        }
    }

}
