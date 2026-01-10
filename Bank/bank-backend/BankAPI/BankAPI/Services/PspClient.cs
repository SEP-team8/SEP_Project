using BankAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BankAPI.Services
{
    public class PspClient : IPspClient
    {
        private readonly HttpClient _httpClient;

        public PspClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> NotifyPaymentStatusAsync(PspPaymentStatusDto dto)
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "https://localhost:7150/api/psp/bank/callback",
                dto
            );

            var redirectUrl = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"PSP callback failed ({response.StatusCode}): {redirectUrl}"
                );

            if (string.IsNullOrWhiteSpace(redirectUrl))
                throw new InvalidOperationException("Callback did not return redirectUrl.");

            return redirectUrl;
        }
    }

}
