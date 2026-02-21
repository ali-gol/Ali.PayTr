# PayTr.Payment

A modular .NET payment integration library for **PayTR** that helps you:

- create and persist payment orders,
- generate secure PayTR token requests,
- process callback notifications safely,
- expose ready-to-map minimal API endpoints,
- dispatch final payment outcomes to your own business handlers.

## Contents

- [Project Structure](#project-structure)
- [How It Works](#how-it-works)
- [Installation & Registration](#installation--registration)
- [Configuration](#configuration)
- [Creating a Payment](#creating-a-payment)
- [Callback & Return Endpoints](#callback--return-endpoints)
- [Handling Final Results with Events](#handling-final-results-with-events)
- [Important Notes](#important-notes)

## Project Structure

This repository is separated into focused modules:

- `PayTr.Payment.Abstractions`
  - Contracts and DTOs (`IPayTrOrderService`, `IPayTrNotificationProcessor`, request/response models, options, enums, events).
- `PayTr.Payment.Core`
  - Main business logic (token generation, order flow, callback processing, event dispatching).
- `PayTr.Payment.EFCore`
  - EF Core repository wiring for persistence.
- `PayTr.Payment.Endpoints`
  - Minimal API endpoint mapping and handlers (`/paytr/notification`, `/paytr/success/{correlationId}`, `/paytr/fail/{correlationId}`).
- `PayTr.Payment` / `WebApplication1`
  - Sample host applications.

## How It Works

1. Your app calls `IPayTrOrderService.CreateOrderAsync(...)` with a `PayTrCreatePaymentRequest`.
2. The library creates/persists the order and requests a PayTR token.
3. You redirect/embed the user to complete payment via the PayTR checkout flow.
4. PayTR posts callback data to `/paytr/notification`.
5. `IPayTrNotificationProcessor` verifies and processes the callback.
6. Final state (`success` / `failed`) is dispatched through `IPayTrOrderEventDispatcher` to all registered `IPayTrOrderEventHandler` implementations.

## Installation & Registration

Register services during startup:

```csharp
using PayTr.Payment.Core.DependencyInjection;
using PayTr.Payment.EFCore.DependencyInjection;
using PayTr.Payment.AspNetCore.DependencyInjection;
using PayTr.Payment.AspNetCore.Routing;

builder.Services.AddPayTrPaymentsCore(builder.Configuration);
builder.Services.AddPayTrPaymentsEFCore<AppDbContext>();
builder.Services.AddPayTrPaymentsEndpoints();

var app = builder.Build();

app.MapPayTrPaymentEndpoints();
```

## Configuration

Add a `PayTr` section in `appsettings.json`:

```json
{
  "PayTr": {
    "MerchantId": "YOUR_MERCHANT_ID",
    "MerchantKey": "YOUR_MERCHANT_KEY",
    "MerchantSalt": "YOUR_MERCHANT_SALT",
    "ServerIp": "127.0.0.1",
    "Currency": "TRY",
    "TestMode": true,
    "Language": "tr",
    "SiteUrl": "https://your-domain.com",
    "BaseUrl": "https://www.paytr.com/"
  }
}
```

## Creating a Payment

Example usage:

```csharp
using PayTr.Payment.Abstractions.Interfaces;
using PayTr.Payment.Abstractions.Models;

public static async Task<IResult> CreatePayment(
    IPayTrOrderService orderService,
    CancellationToken cancellationToken)
{
    var correlationId = Guid.NewGuid();

    var request = new PayTrCreatePaymentRequest
    {
        CorrelationId = correlationId,
        ClientIp = "127.0.0.1",
        CustomerFullName = "Jane Doe",
        CustomerAddress = "Istanbul",
        CustomerEmail = "jane@example.com",
        CustomerPhone = "+905555555555",
        PaymentAmount = 250.00m,
        PaymentType = "card",
        Currency = "TL",
        InstallmentCount = 0,
        OkUrl = $"https://your-domain.com/paytr/success/{correlationId}",
        FailUrl = $"https://your-domain.com/paytr/fail/{correlationId}",
        BasketItems = new List<PayTrBasketItem>
        {
            new() { Name = "Order #1", Price = "250.00", Quantity = 1 }
        }
    };

    var response = await orderService.CreateOrderAsync(request, cancellationToken);

    if (!response.IsSuccess || string.IsNullOrWhiteSpace(response.Token))
        return Results.BadRequest(response.Message ?? "Payment token could not be created.");

    return Results.Ok(response);
}
```

## Callback & Return Endpoints

`MapPayTrPaymentEndpoints()` maps these routes:

- `POST /paytr/notification`
  - Receives PayTR callback form fields.
  - Processes callback through `IPayTrNotificationProcessor`.
- `GET /paytr/success/{correlationId}`
  - Return URL for successful payment navigation.
- `GET /paytr/fail/{correlationId}`
  - Return URL for failed/cancelled payment navigation.

## Handling Final Results with Events

You can plug in one or more custom handlers:

```csharp
using PayTr.Payment.Abstractions.Events;
using PayTr.Payment.Abstractions.Models;
using PayTr.Payment.Core.DependencyInjection;

public sealed class PaymentResultNotifier : IPayTrOrderEventHandler
{
    public Task OnPaymentSucceededAsync(Guid correlationId, OrderPayTrNotificationDto notification)
    {
        // queue / webhook / analytics / mail
        return Task.CompletedTask;
    }

    public Task OnPaymentFailedAsync(Guid correlationId, OrderPayTrNotificationDto notification)
    {
        // queue / webhook / alerting
        return Task.CompletedTask;
    }
}

builder.Services.AddPayTrOrderEventHandler<PaymentResultNotifier>();
// optional lifetime overload:
// builder.Services.AddPayTrOrderEventHandler<PaymentResultNotifier>(ServiceLifetime.Singleton);
```

## Important Notes

- Callback processing should stay idempotent and trusted only after hash verification.
- Keep merchant credentials out of source control (use secrets/environment variables).
- Use `TestMode=true` in non-production environments.
- Ensure your publicly reachable callback URL points to `/paytr/notification`.
- Register multiple event handlers if different systems need payment result propagation.

---

If you want, I can also add:

- a **Turkish documentation version**,
- a **sequence diagram** for order → callback → event flow,
- and a **production checklist** section (timeouts, retry policy, monitoring, dead-letter strategy).
