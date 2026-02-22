using PayTr.Payment.Abstractions.Models;

namespace PayTr.Payment.Abstractions.Events;

public interface IPayTrOrderEventDispatcher
{
    Task DispatchSuccessAsync(Guid correlationId, PayTrPaymentSuccessNotification notification, CancellationToken cancellationToken = default);
    Task DispatchFailureAsync(Guid correlationId, PayTrPaymentFailedNotification notification, CancellationToken cancellationToken = default);
}
