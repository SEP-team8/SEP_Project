using System.Security.Cryptography;
using System.Text;

namespace BankAPI.Helpers.HmacValidator
{
    public class HmacValidator : IHmacValidator
    {
        public bool Validate(string payload, string signature, string secretKey)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computed = Convert.ToHexString(hash).ToLower();

            return computed == signature.ToLower();
        }
    }

}
