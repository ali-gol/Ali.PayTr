using Ali.PayTr.Abstractions.Models;

namespace Ali.PayTr.Abstractions.Events;

public sealed class NullPayTrOrderEventHandler : IPayTrOrderEventHandler
{
    public Task OnPaymentSucceededAsync(Guid correlationId, PayTrPaymentSuccessNotification notification)
        => Task.CompletedTask;

    public Task OnPaymentFailedAsync(Guid correlationId, PayTrPaymentFailedNotification notification)
        => Task.CompletedTask;
}
