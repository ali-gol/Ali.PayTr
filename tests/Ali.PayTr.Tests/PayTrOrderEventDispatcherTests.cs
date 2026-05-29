using Ali.PayTr.Abstractions.Events;
using Ali.PayTr.Abstractions.Models;
using Ali.PayTr.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ali.PayTr.Tests;

public class PayTrOrderEventDispatcherTests
{
    private readonly Mock<ILogger<PayTrOrderEventDispatcher>> _loggerMock;
    private readonly Mock<IPayTrOrderEventHandler> _handlerMock1;
    private readonly Mock<IPayTrOrderEventHandler> _handlerMock2;

    public PayTrOrderEventDispatcherTests()
    {
        _loggerMock = new Mock<ILogger<PayTrOrderEventDispatcher>>();
        _handlerMock1 = new Mock<IPayTrOrderEventHandler>();
        _handlerMock2 = new Mock<IPayTrOrderEventHandler>();
    }

    [Fact]
    public async Task DispatchSuccessAsync_ShouldCallAllHandlers()
    {
        // Arrange
        var handlers = new List<IPayTrOrderEventHandler> { _handlerMock1.Object, _handlerMock2.Object };
        var dispatcher = new PayTrOrderEventDispatcher(handlers, _loggerMock.Object);
        var correlationId = Guid.NewGuid();
        var notification = new PayTrPaymentSuccessNotification();

        // Act
        await dispatcher.DispatchSuccessAsync(correlationId, notification);

        // Assert
        _handlerMock1.Verify(h => h.OnPaymentSucceededAsync(correlationId, notification), Times.Once);
        _handlerMock2.Verify(h => h.OnPaymentSucceededAsync(correlationId, notification), Times.Once);
    }

    [Fact]
    public async Task DispatchFailureAsync_ShouldCallAllHandlers()
    {
        // Arrange
        var handlers = new List<IPayTrOrderEventHandler> { _handlerMock1.Object, _handlerMock2.Object };
        var dispatcher = new PayTrOrderEventDispatcher(handlers, _loggerMock.Object);
        var correlationId = Guid.NewGuid();
        var notification = new PayTrPaymentFailedNotification();

        // Act
        await dispatcher.DispatchFailureAsync(correlationId, notification);

        // Assert
        _handlerMock1.Verify(h => h.OnPaymentFailedAsync(correlationId, notification), Times.Once);
        _handlerMock2.Verify(h => h.OnPaymentFailedAsync(correlationId, notification), Times.Once);
    }

    [Fact]
    public async Task DispatchSuccessAsync_ShouldContinueWhenHandlerThrows()
    {
        // Arrange
        _handlerMock1.Setup(h => h.OnPaymentSucceededAsync(It.IsAny<Guid>(), It.IsAny<PayTrPaymentSuccessNotification>()))
                     .ThrowsAsync(new Exception("Test Exception"));

        var handlers = new List<IPayTrOrderEventHandler> { _handlerMock1.Object, _handlerMock2.Object };
        var dispatcher = new PayTrOrderEventDispatcher(handlers, _loggerMock.Object);
        var correlationId = Guid.NewGuid();
        var notification = new PayTrPaymentSuccessNotification();

        // Act
        await dispatcher.DispatchSuccessAsync(correlationId, notification);

        // Assert
        _handlerMock1.Verify(h => h.OnPaymentSucceededAsync(correlationId, notification), Times.Once);
        _handlerMock2.Verify(h => h.OnPaymentSucceededAsync(correlationId, notification), Times.Once);
    }
}
