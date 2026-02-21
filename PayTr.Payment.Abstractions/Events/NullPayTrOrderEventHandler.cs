using PayTr.Payment.Abstractions.Models;

namespace PayTr.Payment.Abstractions.Events;

public sealed class NullPayTrOrderEventHandler : IPayTrOrderEventHandler
{
    public Task OnPaymentSucceededAsync(Guid correlationId, OrderPayTrNotificationDto notification)
        => Task.CompletedTask;

    public Task OnPaymentFailedAsync(Guid correlationId, OrderPayTrNotificationDto notification)
        => Task.CompletedTask;
}
