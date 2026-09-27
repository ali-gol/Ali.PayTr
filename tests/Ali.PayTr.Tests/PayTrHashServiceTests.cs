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
        var hashStr = "test_string" + "my_salt";
        var merchantKey = "my_secret_key";

        // Act
        var result = service.CreateTokenHash(hashStr, merchantKey);

        // Assert
        var expectedHash = GenerateExpectedHash(hashStr, merchantKey);
        Assert.Equal(expectedHash, result);
    }

    private string GenerateExpectedHash(string hashStr, string merchantKey)
    {
        var key = Encoding.UTF8.GetBytes(merchantKey);
        var message = Encoding.UTF8.GetBytes(hashStr);
        
        using var hmac = new HMACSHA256(key);
        var hashValue = hmac.ComputeHash(message);
        return Convert.ToBase64String(hashValue);
    }
}
