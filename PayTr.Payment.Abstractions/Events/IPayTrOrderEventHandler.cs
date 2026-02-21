using PayTr.Payment.Abstractions.Models;

namespace PayTr.Payment.Abstractions.Events;

public interface IPayTrOrderEventHandler
{
    Task OnPaymentSucceededAsync(Guid correlationId, OrderPayTrNotificationDto notification);
    Task OnPaymentFailedAsync(Guid correlationId, OrderPayTrNotificationDto notification);
}