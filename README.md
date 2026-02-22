# PayTr.Payment

## Event dispatcher pattern for final payment results

The package uses an **interface-based dispatcher pattern** and separate notification models for success and failure.

- `PayTrPaymentSuccessNotification`: sent for successful finalization.
- `PayTrPaymentFailedNotification`: sent for failed finalization (PayTR or system errors).
- `IPayTrOrderEventDispatcher`: dispatches notifications to all handlers.
- `IPayTrOrderEventHandler`: consumer hook for success/failure callbacks.

### Why two failure types?

`PayTrPaymentFailedNotification` includes `IsSystemError` so consumers can distinguish:

- **PayTR failures** (`IsSystemError = false`): gateway business failures.
- **System failures** (`IsSystemError = true`): local processing failures such as hash mismatch or missing order.

System failures include custom codes/messages, for example:
- `SYSTEM_HASH_MISMATCH`
- `SYSTEM_ORDER_NOT_FOUND`

## Usage

```csharp
using PayTr.Payment.Abstractions.Events;
using PayTr.Payment.Abstractions.Models;
using PayTr.Payment.Core.DependencyInjection;

public sealed class PaymentResultNotifier : IPayTrOrderEventHandler
{
    public Task OnPaymentSucceededAsync(Guid correlationId, PayTrPaymentSuccessNotification notification)
    {
        // Handle successful payment
        return Task.CompletedTask;
    }

    public Task OnPaymentFailedAsync(Guid correlationId, PayTrPaymentFailedNotification notification)
    {
        if (notification.IsSystemError)
        {
            // Handle local/system problems (hash mismatch, order not found, etc.)
        }

        // Handle failure notification
        return Task.CompletedTask;
    }
}

builder.Services.AddPayTrPaymentsCore(builder.Configuration);
builder.Services.AddPayTrOrderEventHandler<PaymentResultNotifier>();
```
