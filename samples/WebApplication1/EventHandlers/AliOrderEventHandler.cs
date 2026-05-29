using Ali.PayTr.Abstractions.Events;
using Ali.PayTr.Abstractions.Models;

namespace WebApplication1.EventHandlers
{
    public class AliOrderEventHandler : IPayTrOrderEventHandler
    {
        public Task OnPaymentFailedAsync(Guid correlationId, PayTrPaymentFailedNotification notification)
        {
            // Here you can handle the payment failed event, for example, by logging the failure or updating the order status in your database.
            // or raise event in message queue for other services to consume.
            throw new NotImplementedException();
        }
     
        public Task OnPaymentSucceededAsync(Guid correlationId, PayTrPaymentSuccessNotification notification)
        {
            // Here you can handle the payment success event, for example, by logging the failure or updating the order status in your database.
            // or raise event in message queue for other services to consume.
            throw new NotImplementedException();
        }
    }
}