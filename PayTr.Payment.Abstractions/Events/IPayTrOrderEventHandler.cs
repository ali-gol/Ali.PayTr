using PayTr.Payment.Abstractions.Models;

namespace PayTr.Payment.Abstractions.Events;

public interface IPayTrOrderEventHandler
{
    Task OnPaymentSucceededAsync(Guid correlationId, PayTrPaymentSuccessNotification notification);
    Task OnPaymentFailedAsync(Guid correlationId, PayTrPaymentFailedNotification notification);
}
