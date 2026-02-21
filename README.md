# PayTr.Payment

## Event dispatcher pattern for final payment results

The package now uses an **interface-based event dispatcher pattern** for terminal PayTR results.

- `IPayTrOrderEventHandler`: consumer-implemented handlers.
- `IPayTrOrderEventDispatcher`: library dispatcher that fans out to all registered handlers.

When callback processing finalizes an order (`success`/`failed`), `PayTrNotificationProcessor` dispatches the result through `IPayTrOrderEventDispatcher`.

```csharp
using PayTr.Payment.Abstractions.Events;
using PayTr.Payment.Abstractions.Models;
using PayTr.Payment.Core.DependencyInjection;

public sealed class PaymentResultNotifier : IPayTrOrderEventHandler
{
    public Task OnPaymentSucceededAsync(Guid correlationId, OrderPayTrNotificationDto notification)
    {
        // Push to queue / webhook / email / etc.
        return Task.CompletedTask;
    }

    public Task OnPaymentFailedAsync(Guid correlationId, OrderPayTrNotificationDto notification)
    {
        // Push to queue / webhook / email / etc.
        return Task.CompletedTask;
    }
}

builder.Services.AddPayTrPaymentsCore(builder.Configuration);
builder.Services.AddPayTrOrderEventHandler<PaymentResultNotifier>(); // default Scoped
```

### Notes
- Dispatch happens only once per order finalization to keep processing idempotent.
- Multiple handlers are supported.
- Handler failures are isolated and logged by the dispatcher.
- You can choose handler lifetime with `AddPayTrOrderEventHandler<THandler>(ServiceLifetime)`.
