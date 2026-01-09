using System.Security.Cryptography;

namespace webshop_back.Helpers
{
    public static class ApiKeyHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;

        // Returns stored format: "{saltBase64}.{hashBase64}"
        public static string Hash(string apiKey)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            using var derive = new Rfc2898DeriveBytes(apiKey, salt, Iterations, HashAlgorithmName.SHA256);
            var key = derive.GetBytes(KeySize);
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
        }

        public static bool Verify(string apiKey, string stored)
        {
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(stored)) return false;
            var parts = stored.Split('.');
            if (parts.Length != 2) return false;

            var salt = Convert.FromBase64String(parts[0]);
            var expected = Convert.FromBase64String(parts[1]);

            using var derive = new Rfc2898DeriveBytes(apiKey, salt, Iterations, HashAlgorithmName.SHA256);
            var actual = derive.GetBytes(KeySize);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

        // Convenience: generate raw + stored pair. Use raw to show to merchant only once.
        public static (string Raw, string StoredHash) Generate()
        {
            var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var stored = Hash(raw);
            return (raw, stored);
        }
    }
}
