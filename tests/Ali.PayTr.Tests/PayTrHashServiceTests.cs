using Ali.PayTr.Core.Services;
using System.Security.Cryptography;
using System.Text;

namespace Ali.PayTr.Tests;

public class PayTrHashServiceTests
{
    [Fact]
    public void CreateTokenHash_ShouldGenerateValidHMACSHA256Hash()
    {
        // Arrange
        var service = new PayTrHashService();
        var hashStr = "test_string";
        var merchantKey = "my_secret_key";
        var merchantSalt = "my_salt";

        // Act
        var result = service.CreateTokenHash(hashStr, merchantKey, merchantSalt);

        // Assert
        var expectedHash = GenerateExpectedHash(hashStr, merchantKey, merchantSalt);
        Assert.Equal(expectedHash, result);
    }

    private string GenerateExpectedHash(string hashStr, string merchantKey, string merchantSalt)
    {
        var key = Encoding.UTF8.GetBytes(merchantKey);
        var message = Encoding.UTF8.GetBytes(hashStr + merchantSalt);
        
        using var hmac = new HMACSHA256(key);
        var hashValue = hmac.ComputeHash(message);
        return Convert.ToBase64String(hashValue);
    }
}
