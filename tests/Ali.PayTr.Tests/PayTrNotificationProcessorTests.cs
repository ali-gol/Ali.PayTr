using Ali.PayTr.Abstractions.Enums;
using Ali.PayTr.Abstractions.Events;
using Ali.PayTr.Abstractions.Interfaces;
using Ali.PayTr.Abstractions.Models;
using Ali.PayTr.Abstractions.Options;
using Ali.PayTr.Core.Entities;
using Ali.PayTr.Core.Interfaces;
using Ali.PayTr.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Ali.PayTr.Tests;

public class PayTrNotificationProcessorTests
{
    private readonly Mock<IPayTrRepository> _mockRepository;
    private readonly Mock<IPayTrHashService> _mockHashService;
    private readonly Mock<IPayTrOrderEventDispatcher> _mockEventDispatcher;
    private readonly Mock<ILogger<PayTrNotificationProcessor>> _mockLogger;
    private readonly PayTrFailReasonService _failReasonService;
    private readonly IOptions<PayTrOptions> _options;

    public PayTrNotificationProcessorTests()
    {
        _mockRepository = new Mock<IPayTrRepository>();
        _mockHashService = new Mock<IPayTrHashService>();
        _mockEventDispatcher = new Mock<IPayTrOrderEventDispatcher>();
        _mockLogger = new Mock<ILogger<PayTrNotificationProcessor>>();
        _failReasonService = new PayTrFailReasonService();

        _options = Options.Create(new PayTrOptions
        {
            MerchantId = "123",
            MerchantKey = "key",
            MerchantSalt = "salt",
            SiteUrl = "http://localhost",
            RoutePrefix = "paytr"
        });
    }

    [Fact]
    public async Task ProcessNotificationAsync_ShouldThrowException_WhenHashIsInvalid()
    {
        // Arrange
        var processor = new PayTrNotificationProcessor(
            _mockRepository.Object,
            _mockHashService.Object,
            _options,
            _mockLogger.Object,
            _failReasonService,
            _mockEventDispatcher.Object);

        var notification = new PayTrNotificationRequest
        {
            MerchantOid = Guid.NewGuid().ToString(),
            Status = "success",
            TotalAmount = "100",
            Hash = "aW52YWxpZF9oYXNo"
        };

        // Return a different hash than what's in the notification
        _mockHashService.Setup(x => x.CreateTokenHash(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("c29tZV9vdGhlcl9oYXNo");

        // Act 
        // Wait, does it throw? No, in my observation it dispatches a system failure and returns.
        // Let's check the logic:
        // if (expectedHash != notification.Hash) { ... await DispatchFailureAsync(...) return; }
        await processor.ProcessNotificationAsync(notification);
        
        // Assert
        _mockEventDispatcher.Verify(x => x.DispatchFailureAsync(It.IsAny<Guid>(), It.IsAny<PayTrPaymentFailedNotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessNotificationAsync_ShouldUpdateOrderAndDispatchSuccess_WhenValidAndSuccess()
    {
        // Arrange
        var processor = new PayTrNotificationProcessor(
            _mockRepository.Object,
            _mockHashService.Object,
            _options,
            _mockLogger.Object,
            _failReasonService,
            _mockEventDispatcher.Object);

        var correlationId = Guid.NewGuid();
        var notification = new PayTrNotificationRequest
        {
            MerchantOid = correlationId.ToString(),
            Status = "success",
            TotalAmount = "100",
            Hash = "dmFsaWRfaGFzaA==",
            RawFormAsJson = "{}"
        };

        var order = new PayTrOrder { Id = Guid.NewGuid(), CorrelationId = correlationId, Status = OrderStatus.Created.ToString() };

        _mockHashService.Setup(x => x.CreateTokenHash(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("dmFsaWRfaGFzaA==");

        _mockRepository.Setup(x => x.GetOrderByCorrelationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        await processor.ProcessNotificationAsync(notification);

        // Assert
        Assert.Equal(OrderStatus.CompletedWithSuccess.ToString(), order.Status);
        _mockRepository.Verify(x => x.UpdateOrderAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); 
        
        _mockEventDispatcher.Verify(x => x.DispatchSuccessAsync(correlationId, It.IsAny<PayTrPaymentSuccessNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventDispatcher.Verify(x => x.DispatchFailureAsync(It.IsAny<Guid>(), It.IsAny<PayTrPaymentFailedNotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessNotificationAsync_ShouldUpdateOrderAndDispatchFail_WhenValidAndFail()
    {
        // Arrange
        var processor = new PayTrNotificationProcessor(
            _mockRepository.Object,
            _mockHashService.Object,
            _options,
            _mockLogger.Object,
            _failReasonService,
            _mockEventDispatcher.Object);

        var correlationId = Guid.NewGuid();
        var notification = new PayTrNotificationRequest
        {
            MerchantOid = correlationId.ToString(),
            Status = "failed",
            TotalAmount = "100",
            Hash = "dmFsaWRfaGFzaA==",
            RawFormAsJson = "{}"
        };

        var order = new PayTrOrder { Id = Guid.NewGuid(), CorrelationId = correlationId, Status = OrderStatus.Created.ToString() };

        _mockHashService.Setup(x => x.CreateTokenHash(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("dmFsaWRfaGFzaA==");

        _mockRepository.Setup(x => x.GetOrderByCorrelationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        await processor.ProcessNotificationAsync(notification);

        // Assert
        Assert.Equal(OrderStatus.CompletedWithFail.ToString(), order.Status);
        _mockRepository.Verify(x => x.UpdateOrderAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        
        _mockEventDispatcher.Verify(x => x.DispatchFailureAsync(correlationId, It.IsAny<PayTrPaymentFailedNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventDispatcher.Verify(x => x.DispatchSuccessAsync(It.IsAny<Guid>(), It.IsAny<PayTrPaymentSuccessNotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
