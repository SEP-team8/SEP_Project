using BankAPI.DTOs;
using System.Globalization;

namespace BankAPI.Helpers
{
    public static class IpsQrPayloadGenerator
    {
        public static string Generate(IpsQrData data)
        {
            var amount = data.Amount
                .ToString("0.00", CultureInfo.InvariantCulture)
                .Replace(".", ",");

            var payload =
                $"K:PR|" +
                $"V:01|" +
                $"C:1|" +
                $"R:{data.MerchantAccount}|" +
                $"N:{data.MerchantName}|" +
                $"I:RSD{amount}|" +
                $"SF:289|" +
                $"S:Placanje robe";


            return payload;
        }
    }
}
