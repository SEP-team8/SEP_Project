using CryptoService.Clients.Interfaces;
using CryptoService.DTOs.Binance;

namespace CryptoService.Clients;

public sealed class BinanceClient : IBinanceClient
{
    private readonly HttpClient _httpClient;

    public BinanceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<decimal> GetBitcoinPriceAsync(string symbol, CancellationToken cancellationToken)
    {
        // Binance public price endpoint
        string url = $"https://api.binance.com/api/v3/ticker/price?symbol={symbol.ToUpper()}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "CryptoSchoolProject/1.0");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        BinancePriceDto result = await response.Content.ReadFromJsonAsync<BinancePriceDto>(cancellationToken)
                     ?? throw new Exception("Failed to deserialize Binance response");

        if (!decimal.TryParse(result.Price, out var price))
        {
            throw new Exception("Invalid price returned from Binance");
        }

        return price;
    }
}
