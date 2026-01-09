using System.Security.Cryptography;
using System.Text;
using PSPbackend.DTOs.Bank;

namespace PSPbackend.Helpers
{
    public static class SignatureHelper
    {
        public static string BuildPayload(InitPaymentRequestDto dto, DateTime timestampUtcHeader)
        {
            return
                $"merchantId={dto.MerchantId}&amount={dto.Amount}" +
                $"&currency={(int)dto.Currency}&stan={dto.Stan}" +
                $"&timestamp={timestampUtcHeader:o}";
        }
        public static string CreateSignature(string payload, string secretKey)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
