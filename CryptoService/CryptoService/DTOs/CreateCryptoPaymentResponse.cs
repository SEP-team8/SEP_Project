namespace CryptoService.DTOs;

public sealed record CreateCryptoPaymentResponse(
    Guid PaymentId,
    string BitcoinAddress,
    decimal BitcoinAmount,
    DateTime ExpiresAt
);
