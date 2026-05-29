using Ali.PayTr.Abstractions.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Ali.PayTr.Core.Services;


public sealed class PayTrHashService : IPayTrHashService
{
    public string CreateTokenHash(string hashStr, string merchantKey, string merchantSalt)
    {
        var key = Encoding.UTF8.GetBytes(merchantKey);
        var message = Encoding.UTF8.GetBytes(hashStr + merchantSalt);
        
        using var hmac = new HMACSHA256(key);
        var hashValue = hmac.ComputeHash(message);
        return Convert.ToBase64String(hashValue);
    }
}