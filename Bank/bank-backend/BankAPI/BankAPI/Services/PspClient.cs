using BankAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BankAPI.Services
{
    public class PspClient : IPspClient
    {
        private readonly HttpClient _httpClient;
        private readonly Microsoft.Extensions.Logging.ILogger<PspClient> _logger;

        public PspClient(HttpClient httpClient, Microsoft.Extensions.Logging.ILogger<PspClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> NotifyPaymentStatusAsync(PspPaymentStatusDto dto)
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "/api/psp/bank/callback",
                dto
            );

            var redirectUrl = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogError("PSP callback failed ({StatusCode}): {Response}", response.StatusCode, redirectUrl);
                throw new InvalidOperationException(
                    $"PSP callback failed ({response.StatusCode}): {redirectUrl}"
                );
            }

            if (string.IsNullOrWhiteSpace(redirectUrl))
                throw new InvalidOperationException("Callback did not return redirectUrl.");

            return redirectUrl;
        }
    }

}
