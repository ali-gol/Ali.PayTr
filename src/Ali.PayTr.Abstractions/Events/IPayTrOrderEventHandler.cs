using Ali.PayTr.Abstractions.Models;

namespace Ali.PayTr.Abstractions.Events;

public interface IPayTrOrderEventHandler
{
    Task OnPaymentSucceededAsync(Guid correlationId, PayTrPaymentSuccessNotification notification);
    Task OnPaymentFailedAsync(Guid correlationId, PayTrPaymentFailedNotification notification);
}
