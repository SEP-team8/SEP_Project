namespace CryptoService.DTOs;

public sealed record CryptoPaymentStatusResponse(
    Guid PaymentId,
    CryptoService.Models.CryptoPaymentStatus Status,
    decimal EthAmount,
    string? TransactionHash,
    int Confirmations
);
