using Ali.PayTr.Abstractions.Interfaces;
using Ali.PayTr.Abstractions.Models;
using Ali.PayTr.Abstractions.Options;
using Ali.PayTr.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;

namespace Ali.PayTr.Tests;
public class PayTrRefundServiceTests
{
    private readonly Mock<IPayTrHashService> _hashServiceMock;
    private readonly Mock<ILogger<PayTrRefundService>> _loggerMock;
    private readonly IOptions<PayTrOptions> _options;

    public PayTrRefundServiceTests()
    {
        _hashServiceMock = new Mock<IPayTrHashService>();
        _loggerMock = new Mock<ILogger<PayTrRefundService>>();
        _options = Options.Create(new PayTrOptions
        {
            MerchantId = "TestMerchant",
            MerchantKey = "TestKey",
            MerchantSalt = "TestSalt",
            SiteUrl = "http://localhost",
            RoutePrefix = "paytr"
        });
    }

    [Fact]
    public async Task RefundAsync_ShouldReturnSuccess_WhenApiReturnsSuccess()
    {
        // Arrange
        var jsonResponse = @"{
                ""status"": ""success"",
                ""is_test"": 1,
                ""merchant_oid"": ""some_oid"",
                ""return_amount"": ""50.00""
            }";

        var httpClient = CreateMockHttpClient(jsonResponse, HttpStatusCode.OK);
        var service = new PayTrRefundService(httpClient, _options, _hashServiceMock.Object, _loggerMock.Object);

        var request = new PayTrRefundRequest 
        { 
            CorrelationId = Guid.NewGuid(),
            ReturnAmount = 50.00m,
            ReferenceNo = "ref123"
        };

        _hashServiceMock.Setup(h => h.CreateTokenHash(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .Returns("hashed_token");

        // Act
        var result = await service.RefundAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("50.00", result.RefundAmount);
    }

    [Fact]
    public async Task RefundAsync_ShouldReturnFail_WhenApiReturnsError()
    {
        // Arrange
        var jsonResponse = @"{
                ""status"": ""error"",
                ""err_msg"": ""Insufficient balance""
            }";

        var httpClient = CreateMockHttpClient(jsonResponse, HttpStatusCode.OK);
        var service = new PayTrRefundService(httpClient, _options, _hashServiceMock.Object, _loggerMock.Object);

        var request = new PayTrRefundRequest { CorrelationId = Guid.NewGuid(), ReturnAmount = 50m };

        _hashServiceMock.Setup(h => h.CreateTokenHash(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .Returns("hashed_token");

        // Act
        var result = await service.RefundAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Insufficient balance", result.Message);
    }

    private HttpClient CreateMockHttpClient(string responseContent, HttpStatusCode statusCode)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent)
            });

        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://www.paytr.com/")
        };
    }
}
