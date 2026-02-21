namespace PayTr.Payment.Abstractions.Interfaces;

public interface IPayTrHashService
{
    string CreateTokenHash(string hashStr, string merchantKey, string merchantSalt);
}
