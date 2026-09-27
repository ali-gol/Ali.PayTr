using Ali.PayTr.Abstractions.Models;
using Ali.PayTr.Abstractions.Options;
using Ali.PayTr.Core.Clients;
using Ali.PayTr.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace Ali.PayTr.Tests;

public class PayTrDirectClientTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly PayTrOptions _options;
    private readonly Mock<ILogger<PayTrDirectClient>> _loggerMock;
    private readonly PayTrHashService _hashService;

    public PayTrDirectClientTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://www.paytr.com/")
        };
        _options = new PayTrOptions { MerchantId = "MID", MerchantKey = "MKEY", MerchantSalt = "MSALT" };
        _loggerMock = new Mock<ILogger<PayTrDirectClient>>();
        _hashService = new PayTrHashService();
    }

    private PayTrDirectClient CreateClient()
    {
        var optionsMock = new Mock<IOptions<PayTrOptions>>();
        optionsMock.Setup(o => o.Value).Returns(_options);
        return new PayTrDirectClient(_httpClient, optionsMock.Object, _hashService, _loggerMock.Object);
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldMaskCardNumber_InLogger()
    {
        // Arrange
        var responseContent = JsonSerializer.Serialize(new PayTrDirectPaymentResponse { Status = "success" });
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        var client = CreateClient();
        var request = new PayTrDirectPaymentRequest
        {
            UserIp = "1.1.1.1", MerchantOid = "OID", Email = "a@a.com", PaymentAmount = 100,
            CcOwner = "Test", CardNumber = "1234567890123456", Cvv = "123", ExpiryMonth = "12", ExpiryYear = "25",
            MerchantOkUrl = "ok", MerchantFailUrl = "fail", UserName = "U", UserAddress = "A", UserPhone = "P"
        };

        // Act
        var result = await client.CreatePaymentAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        
        // Verify logger was called with masked card number
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("123456******3456")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
            
        // Verify unmasked card is NOT logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("1234567890123456")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Never);
    }
    
    [Fact]
    public async Task CreatePaymentAsync_ShouldParseSyncModeResponse()
    {
        // Arrange
        var responseContent = JsonSerializer.Serialize(new PayTrDirectPaymentResponse { Status = "wait_callback", Msg = "Waiting for 3D Secure" });
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        var client = CreateClient();
        var request = new PayTrDirectPaymentRequest
        {
            UserIp = "1.1.1.1", MerchantOid = "OID", Email = "a@a.com", PaymentAmount = 100,
            CcOwner = "Test", CardNumber = "1234567890123456", Cvv = "123", ExpiryMonth = "12", ExpiryYear = "25",
            MerchantOkUrl = "ok", MerchantFailUrl = "fail", UserName = "U", UserAddress = "A", UserPhone = "P",
            SyncMode = true
        };

        // Act
        var result = await client.CreatePaymentAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("wait_callback", result.Response?.Status);
        Assert.Equal("Waiting for 3D Secure", result.Message);
    }
}
