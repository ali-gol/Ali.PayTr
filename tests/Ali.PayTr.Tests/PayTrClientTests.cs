using Ali.PayTr.Abstractions.Interfaces;
using Ali.PayTr.Abstractions.Models;
using Ali.PayTr.Abstractions.Options;
using Ali.PayTr.Core.Clients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System;
using System.Collections.Generic;

namespace Ali.PayTr.Tests
{
    public class PayTrClientTests
    {
        private readonly Mock<IPayTrHashService> _hashServiceMock;
        private readonly Mock<ILogger<PayTrClient>> _loggerMock;
        private readonly IOptions<PayTrOptions> _options;

        public PayTrClientTests()
        {
            _hashServiceMock = new Mock<IPayTrHashService>();
            _loggerMock = new Mock<ILogger<PayTrClient>>();
            _options = Options.Create(new PayTrOptions
            {
                MerchantId = "TestMerchant",
                MerchantKey = "TestKey",
                MerchantSalt = "TestSalt",
                SiteUrl = "http://localhost",
                SuccessUrlPattern = "/success/{correlationId}",
                FailUrlPattern = "/fail/{correlationId}",
                RoutePrefix = "paytr",
                TestMode = true
            });
        }

        [Fact]
        public async Task CreatePaymentAsync_ShouldReturnSuccess_WhenApiReturnsSuccess()
        {
            // Arrange
            var jsonResponse = @"{
                ""status"": ""success"",
                ""token"": ""mock_token_123""
            }";

            var httpClient = CreateMockHttpClient(jsonResponse, HttpStatusCode.OK);
            var client = new PayTrClient(httpClient, _options, _hashServiceMock.Object, _loggerMock.Object);

            var request = new PayTrCreatePaymentRequest
            {
                CorrelationId = Guid.NewGuid(),
                ClientIp = "127.0.0.1",
                PaymentAmount = 100.50m,
                CustomerEmail = "test@test.com",
                CustomerFullName = "Test User",
                CustomerAddress = "Test Address",
                CustomerPhone = "1234567890",
                Currency = "TRY",
                InstallmentCount = 1,
                BasketItems = new List<PayTrBasketItem> { new() { Name = "Item", Price = 100.50m, Quantity = 1 } }
            };

            _hashServiceMock.Setup(h => h.CreateTokenHash(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                            .Returns("hashed_token");

            // Act
            var result = await client.CreatePaymentAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("mock_token_123", result.Token);
            Assert.Equal("https://www.paytr.com/odeme/guvenli/mock_token_123", result.RedirectUrl);
            Assert.Equal(request.CorrelationId, result.CorrelationId);
        }

        [Fact]
        public async Task CreatePaymentAsync_ShouldReturnFail_WhenApiReturnsError()
        {
            // Arrange
            var jsonResponse = @"{
                ""status"": ""error"",
                ""reason"": ""Invalid credentials""
            }";

            var httpClient = CreateMockHttpClient(jsonResponse, HttpStatusCode.OK);
            var client = new PayTrClient(httpClient, _options, _hashServiceMock.Object, _loggerMock.Object);

            var request = new PayTrCreatePaymentRequest
            {
                CorrelationId = Guid.NewGuid(),
                ClientIp = "127.0.0.1",
                PaymentAmount = 100m,
                CustomerEmail = "test@test.com",
                CustomerFullName = "Test User",
                CustomerAddress = "Test Address",
                CustomerPhone = "1234567890",
                Currency = "TRY",
                BasketItems = new List<PayTrBasketItem> { new() { Name = "Item", Price = 100m, Quantity = 1 } }
            };

            _hashServiceMock.Setup(h => h.CreateTokenHash(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                            .Returns("hashed_token");

            // Act
            var result = await client.CreatePaymentAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid credentials", result.Message);
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
}
