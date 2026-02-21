# PayTr.Payment

## Client notification mechanism

When PayTR callback processing reaches a **final payment state** (`success` or `failed`), the library notifies consumers through interface-based handlers.

Implement `IPayTrOrderEventHandler` in your application and register it using `AddPayTrOrderEventHandler<THandler>()`.

```csharp
using PayTr.Payment.Abstractions.Events;
using PayTr.Payment.Abstractions.Models;
using PayTr.Payment.Core.DependencyInjection;

public sealed class PaymentResultNotifier : IPayTrOrderEventHandler
{
    public Task OnPaymentSucceededAsync(Guid correlationId, OrderPayTrNotificationDto notification)
    {
        // Notify your system (queue, webhook, email, etc.)
        return Task.CompletedTask;
    }

    public Task OnPaymentFailedAsync(Guid correlationId, OrderPayTrNotificationDto notification)
    {
        // Notify your system (queue, webhook, email, etc.)
        return Task.CompletedTask;
    }
}

builder.Services.AddPayTrPaymentsCore(builder.Configuration);
builder.Services.AddPayTrOrderEventHandler<PaymentResultNotifier>();
```

### Notes
- Handlers are triggered only once when the order becomes final (`Success`/`Failed`) to keep callback processing idempotent.
- Multiple handlers are supported.
- Handler failures are logged and do not break PayTR callback processing.
