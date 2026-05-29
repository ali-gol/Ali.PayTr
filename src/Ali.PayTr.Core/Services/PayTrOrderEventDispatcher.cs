using Ali.PayTr.Abstractions.Events;
using Ali.PayTr.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace Ali.PayTr.Core.Services;

public sealed class PayTrOrderEventDispatcher : IPayTrOrderEventDispatcher
{
    private readonly IEnumerable<IPayTrOrderEventHandler> _eventHandlers;
    private readonly ILogger<PayTrOrderEventDispatcher> _logger;

    public PayTrOrderEventDispatcher(
        IEnumerable<IPayTrOrderEventHandler> eventHandlers,
        ILogger<PayTrOrderEventDispatcher> logger)
    {
        _eventHandlers = eventHandlers;
        _logger = logger;
    }

    public async Task DispatchSuccessAsync(Guid correlationId, PayTrPaymentSuccessNotification notification, CancellationToken cancellationToken = default)
    {
        foreach (var eventHandler in _eventHandlers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await eventHandler.OnPaymentSucceededAsync(correlationId, notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while dispatching success notification to {HandlerType} for order {CorrelationId}.",
                    eventHandler.GetType().FullName,
                    correlationId);
            }
        }
    }

    public async Task DispatchFailureAsync(Guid correlationId, PayTrPaymentFailedNotification notification, CancellationToken cancellationToken = default)
    {
        foreach (var eventHandler in _eventHandlers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await eventHandler.OnPaymentFailedAsync(correlationId, notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while dispatching failed notification to {HandlerType} for order {CorrelationId}.",
                    eventHandler.GetType().FullName,
                    correlationId);
            }
        }
    }
}
