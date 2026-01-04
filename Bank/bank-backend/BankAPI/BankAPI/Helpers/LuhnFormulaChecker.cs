namespace BankAPI.Helpers
{
    public static class LuhnFormulaChecker
    {
        public static bool IsValidLuhn(string cardNumber)
        {
            var digits = cardNumber.Replace(" ", "");
            int sum = 0;
            bool alternate = false;

            for (int i = digits.Length - 1; i >= 0; i--)
            {
                int n = digits[i] - '0';

                if (alternate)
                {
                    n *= 2;
                    if (n > 9) n -= 9;
                }

                sum += n;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }
    }
}
