using PayTr.Payment.Abstractions.Models;

namespace PayTr.Payment.Abstractions.Events;

public interface IPayTrOrderEventDispatcher
{
    Task DispatchAsync(Guid correlationId, OrderPayTrNotificationDto notification, CancellationToken cancellationToken = default);
}
