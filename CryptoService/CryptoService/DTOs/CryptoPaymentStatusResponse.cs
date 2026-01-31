using CryptoService.Models;

namespace CryptoService.DTOs;

public sealed record CryptoPaymentStatusResponse(
    Guid PaymentId,
    CryptoPaymentStatus Status,
    decimal BitcoinAmount,
    string? TransactionId,
    int Confirmations
);
