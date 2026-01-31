namespace CryptoService.Clients.Interfaces;

public interface IBinanceClient
{
    /// <summary>
    /// Gets the current BTC price for a given symbol (e.g., "BTCEUR", "BTCUSDT").
    /// </summary>
    Task<decimal> GetBitcoinPriceAsync(string symbol, CancellationToken cancellationToken);
}
