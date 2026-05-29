using Ali.PayTr.Abstractions.Interfaces;
using Ali.PayTr.Abstractions.Models;
using Ali.PayTr.Core.Interfaces;
using Ali.PayTr.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ali.PayTr.Tests;
public class PayTrOrderServiceTests
{
    private readonly Mock<IPayTrRepository> _mockRepository;
    private readonly Mock<IPayTrClient> _mockClient;
    private readonly Mock<ILogger<PayTrOrderService>> _mockLogger;

    public PayTrOrderServiceTests()
    {
        _mockRepository = new Mock<IPayTrRepository>();
        _mockClient = new Mock<IPayTrClient>();
        _mockLogger = new Mock<ILogger<PayTrOrderService>>();
    }

    [Fact]
    public async Task CreateOrderAndGetPaymentUrlAsync_ShouldReturnSuccess_WhenClientSucceeds()
    {

        var service = new PayTrOrderService(_mockClient.Object, _mockRepository.Object, _mockLogger.Object);

        var request = new PayTrCreatePaymentRequest
        {
            CorrelationId = Guid.NewGuid(),
            ClientIp = "127.0.0.1",
            PaymentAmount = 100,
            CustomerEmail = "test@test.com",
            CustomerFullName = "Test User",
            CustomerAddress = "Test Address",
            CustomerPhone = "123456",
            Currency = "TRY",
            BasketItems = new List<PayTrBasketItem> { new() { Name = "Item", Price = 100, Quantity = 1 } }
        };

        var mockResponse = new PayTrCreatePaymentResponse
        {
            IsSuccess = true,
            RedirectUrl = "https://paytr.com/token123"
        };

        _mockClient
            .Setup(x => x.CreatePaymentAsync(It.IsAny<PayTrCreatePaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);


        var result = await service.CreateOrderAndGetPaymentUrlAsync(request);


        Assert.True(result.IsSuccess);
        Assert.Equal("https://paytr.com/token123", result.RedirectUrl);

        _mockRepository.Verify(x => x.AddOrderAsync(It.Is<Ali.PayTr.Core.Entities.PayTrOrder>(o =>
            o.CorrelationId == request.CorrelationId &&
            o.TotalAmount == request.PaymentAmount &&
            o.CustomerEmail == request.CustomerEmail &&
            o.Currency == request.Currency &&
            o.Status == Ali.PayTr.Abstractions.Enums.OrderStatus.Created.ToString()
        ), It.IsAny<CancellationToken>()), Times.Once);

        _mockRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2)); 
        _mockRepository.Verify(x => x.AddLogAsync(It.IsAny<Ali.PayTr.Core.Entities.PayTrOrderLogHistory>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CreateOrderAndGetPaymentUrlAsync_ShouldReturnFail_WhenClientFails()
    {

        var service = new PayTrOrderService(_mockClient.Object, _mockRepository.Object, _mockLogger.Object);

        var request = new PayTrCreatePaymentRequest
        {
            CorrelationId = Guid.NewGuid(),
            ClientIp = "127.0.0.1",
            PaymentAmount = 100,
            CustomerEmail = "test@test.com",
            CustomerFullName = "Test User",
            CustomerAddress = "Test Address",
            CustomerPhone = "123456",
            Currency = "TRY",
            BasketItems = new List<PayTrBasketItem> { new() { Name = "Item", Price = 100, Quantity = 1 } }
        };

        var mockResponse = new PayTrCreatePaymentResponse
        {
            IsSuccess = false,
            Message = "API Error"
        };

        _mockClient
            .Setup(x => x.CreatePaymentAsync(It.IsAny<PayTrCreatePaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);


        var result = await service.CreateOrderAndGetPaymentUrlAsync(request);


        Assert.False(result.IsSuccess);
        Assert.Equal("API Error", result.Message);
    }
}
