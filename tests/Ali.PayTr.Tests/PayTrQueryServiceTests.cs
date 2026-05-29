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
public class PayTrQueryServiceTests
{
    private readonly Mock<IPayTrHashService> _hashServiceMock;
    private readonly Mock<ILogger<PayTrQueryService>> _loggerMock;
    private readonly IOptions<PayTrOptions> _options;

    public PayTrQueryServiceTests()
    {
        _hashServiceMock = new Mock<IPayTrHashService>();
        _loggerMock = new Mock<ILogger<PayTrQueryService>>();
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
    public async Task QueryStatusAsync_ShouldReturnSuccess_WhenApiReturnsSuccess()
    {
        // Arrange
        var jsonResponse = @"{
                ""status"": ""success"",
                ""payment_status"": ""SUCCESS"",
                ""payment_amount"": ""10000"",
                ""failed_reason_code"": null,
                ""failed_reason_msg"": null
            }";

        var httpClient = CreateMockHttpClient(jsonResponse, HttpStatusCode.OK);
        var service = new PayTrQueryService(httpClient, _options, _hashServiceMock.Object, _loggerMock.Object);

        var correlationId = Guid.NewGuid();
        var request = new PayTrQueryRequest { CorrelationId = correlationId };

        _hashServiceMock.Setup(h => h.CreateTokenHash(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .Returns("hashed_token");

        // Act
        var result = await service.QueryStatusAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("SUCCESS", result.PaymentStatus);
        Assert.Equal(100m, result.PaymentAmount); // 10000 / 100 = 100
    }

    [Fact]
    public async Task QueryStatusAsync_ShouldReturnFail_WhenApiReturnsError()
    {
        // Arrange
        var jsonResponse = @"{
                ""status"": ""error"",
                ""err_msg"": ""Invalid hash""
            }";

        var httpClient = CreateMockHttpClient(jsonResponse, HttpStatusCode.OK);
        var service = new PayTrQueryService(httpClient, _options, _hashServiceMock.Object, _loggerMock.Object);

        var request = new PayTrQueryRequest { CorrelationId = Guid.NewGuid() };

        _hashServiceMock.Setup(h => h.CreateTokenHash(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .Returns("hashed_token");

        // Act
        var result = await service.QueryStatusAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid hash", result.Message);
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
