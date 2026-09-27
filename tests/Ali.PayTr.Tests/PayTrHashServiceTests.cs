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

    [Fact]
    public void CreateDirectApiToken_ConstructsCorrectHash_WithExactOrder()
    {
        // Arrange
        var service = new PayTrHashService();
        var options = new Abstractions.Options.PayTrOptions { MerchantId = "MID", MerchantKey = "MKEY", MerchantSalt = "MSALT", TestMode = true };
        var request = new Abstractions.Models.PayTrDirectPaymentRequest
        {
            UserIp = "1.1.1.1",
            MerchantOid = "OID-123",
            Email = "test@test.com",
            PaymentAmount = 100.99m,
            PaymentType = "card",
            InstallmentCount = 0,
            Currency = "TL",
            TestMode = true,
            Non3D = false,
            CcOwner = "John", CardNumber = "1234", Cvv = "123", ExpiryMonth = "12", ExpiryYear = "25",
            MerchantOkUrl = "ok", MerchantFailUrl = "fail",
            UserName = "John", UserAddress = "Addr", UserPhone = "123"
        };

        // Act
        var token = service.CreateDirectApiToken(request, options);

        // Assert
        // merchant_id + user_ip + merchant_oid + email + payment_amount + payment_type + installment_count + currency + test_mode + non_3d + merchant_salt
        var expectedHashStr = "MID" + "1.1.1.1" + "OID-123" + "test@test.com" + "100.99" + "card" + "0" + "TL" + "1" + "0" + "MSALT";
        var expectedToken = GenerateExpectedHash(expectedHashStr, "MKEY");
        Assert.Equal(expectedToken, token);
    }

    [Fact]
    public void CreateDirectApiToken_UsesInvariantCulture_ForDecimals()
    {
        // Arrange
        var service = new PayTrHashService();
        var options = new Abstractions.Options.PayTrOptions { MerchantId = "MID", MerchantKey = "MKEY", MerchantSalt = "MSALT", TestMode = true };
        var request = new Abstractions.Models.PayTrDirectPaymentRequest
        {
            UserIp = "1.1.1.1", MerchantOid = "OID", Email = "a@a.com",
            PaymentAmount = 1234.56m,
            CcOwner = "A", CardNumber = "1", Cvv = "1", ExpiryMonth = "1", ExpiryYear = "1",
            MerchantOkUrl = "a", MerchantFailUrl = "b",
            UserName = "A", UserAddress = "B", UserPhone = "C"
        };
        var currentCulture = Thread.CurrentThread.CurrentCulture;
        
        try
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("tr-TR"); // decimal separator is comma

            // Act
            var token = service.CreateDirectApiToken(request, options);

            // Assert
            var expectedHashStr = "MID" + "1.1.1.1" + "OID" + "a@a.com" + "1234.56" + "card" + "0" + "TL" + "1" + "0" + "MSALT";
            var expectedToken = GenerateExpectedHash(expectedHashStr, "MKEY");
            Assert.Equal(expectedToken, token);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = currentCulture;
        }
    }
}
