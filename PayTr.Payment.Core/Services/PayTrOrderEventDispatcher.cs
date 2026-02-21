using Microsoft.Extensions.Logging;
using PayTr.Payment.Abstractions.Events;
using PayTr.Payment.Abstractions.Models;

namespace PayTr.Payment.Core.Services;

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

    public async Task DispatchAsync(Guid correlationId, OrderPayTrNotificationDto notification, CancellationToken cancellationToken = default)
    {
        foreach (var eventHandler in _eventHandlers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (string.Equals(notification.status, "success", StringComparison.OrdinalIgnoreCase))
                {
                    await eventHandler.OnPaymentSucceededAsync(correlationId, notification);
                    continue;
                }

                await eventHandler.OnPaymentFailedAsync(correlationId, notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while dispatching payment result to {HandlerType} for order {CorrelationId}.",
                    eventHandler.GetType().FullName,
                    correlationId);
            }
        }
    }
}
