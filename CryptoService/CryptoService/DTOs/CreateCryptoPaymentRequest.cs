using CryptoService.Models;

namespace CryptoService.DTOs;

public sealed record CreateCryptoPaymentRequest(Guid OrderId, decimal FiatAmount, Currency FiatCurrency);
