using System;
using System.Security.Cryptography;
using System.Text;

namespace webshop_back.Helpers
{
    public static class SignatureHelper
    {
        // Accepts signature in hex or base64 (we assume base64 here). Adapt if your PSP uses hex.
        public static bool VerifyHmacSha256(string? signatureHeader, string payload, string secret)
        {
            if (string.IsNullOrEmpty(signatureHeader) || string.IsNullOrEmpty(secret)) return false;

            // compute HMAC
            var key = Encoding.UTF8.GetBytes(secret);
            using var hmac = new HMACSHA256(key);
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedB64 = Convert.ToBase64String(computed);

            // compare in fixed time
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(computedB64),
                TryParseBase64(signatureHeader) ?? Array.Empty<byte>());
        }

        private static byte[]? TryParseBase64(string s)
        {
            try { return Convert.FromBase64String(s); }
            catch { return null; }
        }
    }
}
