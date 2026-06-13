namespace BankAPI.Services.CardProtector
{
    public interface ICardProtector
    {
        string ComputePanHash(string pan);
        string ProtectCvv(string cvv);
        string UnprotectCvv(string encryptedCvv);
    }
}
