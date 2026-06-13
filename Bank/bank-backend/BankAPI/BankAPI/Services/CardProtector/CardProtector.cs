using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;

namespace BankAPI.Services.CardProtector
{
    public class CardProtector : ICardProtector
    {
        private readonly IDataProtector _protector;

        public CardProtector(IDataProtectionProvider dataProtectionProvider)
        {
            _protector = dataProtectionProvider.CreateProtector("BankAPI.CardCvv");
        }

        public string ComputePanHash(string pan)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(pan));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        public string ProtectCvv(string cvv)
        {
            return _protector.Protect(cvv);
        }

        public string UnprotectCvv(string encryptedCvv)
        {
            return _protector.Unprotect(encryptedCvv);
        }
    }
}
