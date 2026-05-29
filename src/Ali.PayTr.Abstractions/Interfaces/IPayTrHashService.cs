namespace Ali.PayTr.Abstractions.Interfaces;

public interface IPayTrHashService
{
    string CreateTokenHash(string hashStr, string merchantKey, string merchantSalt);
}
