namespace CryptoService.DTOs;

public sealed record CreateCryptoPaymentRequest(
        Guid MerchantId,
        decimal FiatAmount,
        int Currency,        // numeric enum value (mapira se na CryptoService.Models.Currency)
        string Stan,
        DateTime PspTimestamp
    );
