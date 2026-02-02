namespace CryptoService.DTOs;

public sealed record CreateCryptoPaymentResponse(
    Guid PaymentId,
    string EthAddress,
    decimal EthAmount,
    DateTime ExpiresAt
);
