namespace BankAPI.Helpers.HmacValidator
{
    public interface IHmacValidator
    {
        bool Validate(
            string payload,
            string signature,
            string secretKey
        );
    }

}
