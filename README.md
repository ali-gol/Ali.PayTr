# 🇹🇷 Ali.PayTr.Payment (Türkçe)

**PayTR** için modüler, temiz mimariye sahip bir .NET ödeme entegrasyon kütüphanesi. Bu kütüphane, PayTR entegrasyonunun karmaşıklıklarını (hash oluşturma, webhook doğrulama ve veritabanı takibi) soyutlayarak, geliştiricilere son derece basit, tak-çalıştır bir deneyim sunar.

## Özellikler

✅ iFrame Checkout Entegrasyonu

✅ Webhook / Notification Doğrulaması

✅ **En Basit Entegrasyon Deneyimi**: Tek bir metod çağrısıyla sipariş oluşturun ve yönlendirme (redirect) URL'sini alın.

✅ **Otomatik Webhook İşleme**: PayTR'ın sunucudan-sunucuya bildirimlerini (`/paytr/notification`) otomatik olarak işler, HMAC hashlerini doğrular ve idempotentliği (tekrarsızlığı) güvenle kontrol eder.

✅ **Yapılandırılabilir Yönlendirme (Routing)**: Yapılandırılabilir bir `RoutePrefix` (örn. `/api/payments/paytr`) ile yerleşik Minimal API'lar sunar.

✅ **Veri Kalıcılığı Esnekliği**: Kendi içinde bir Entity Framework Core uygulamasıyla birlikte gelir, ancak sadece `IPayTrRepository` arayüzünü uygulayarak tam bir özelleştirmeye (örn. MongoDB, Dapper) olanak tanır.

✅ **Olay Güdümlü Mimari (Event-Driven)**: `IPayTrOrderEventHandler` arayüzünü sunarak, iş mantığınızın ödeme altyapısından tamamen soyutlanmasını (decouple) sağlar.

✅ **Hata Korumalı Yapılandırma**: ASP.NET Core `IOptions` ve DataAnnotations kullanarak, yapılandırmanızı uygulamanız başlar başlamaz doğrular.

## Kurulum ve Kayıt

Gerekli paketi NuGet üzerinden yükleyin:
```bash
dotnet add package Ali.PayTr
```
*(Bu paket Core, EFCore ve AspNetCore kütüphanelerini içerir. İsterseniz `Ali.PayTr.Core` paketini tek başına yükleyerek kendi veritabanı altyapınızı kullanabilirsiniz.)*

Servisleri uygulamanız başlarken (`Program.cs`) kaydedin:

```csharp
using Ali.PayTr.Core.DependencyInjection;
using Ali.PayTr.EFCore.DependencyInjection;
using Ali.PayTr.AspNetCore.EndpointMapping;

// 1. PayTr Core'u ekleyin (yapılandırma, hash işlemleri, istemciler ve servisleri yönetir)
builder.Services.AddPayTrPaymentsCore(builder.Configuration);

// 2. Entity Framework Core Repository'yi ekleyin (veya kendi IPayTrRepository'nizi kaydedin)
builder.Services.AddPayTrPaymentsEFCore<AppDbContext>();

// 3. Başarılı/Başarısız ödemeleri dinlemek için kendi özel event handler'ınızı kaydedin
builder.Services.AddPayTrOrderEventHandler<MyOrderEventHandler>();

var app = builder.Build();

// 4. Webhook ve dönüş (return) endpoint'lerini eşleştirin
app.MapPayTrPaymentEndpoints();

app.Run();
```

## Yapılandırma

Paket, standart ASP.NET Core yapılandırma sisteminden yararlanır. Bu ayarları `appsettings.json`, `secrets.json` (User Secrets) veya Ortam Değişkenleri (Environment Variables) aracılığıyla tanımlayabilirsiniz.

**Önemli**: Yerleşik başlangıç doğrulaması sayesinde, yapılandırmanızdan herhangi bir `[Required]` özellik eksikse (User Secrets veya Ortam Değişkenleri dahil), uygulamanız üretim ortamında sessiz hataları önlemek için başlarken hemen bir exception fırlatır!

Örnek `appsettings.json`:
```json
{
  "PayTr": {
    "MerchantId": "YOUR_MERCHANT_ID",
    "MerchantKey": "YOUR_MERCHANT_KEY",
    "MerchantSalt": "YOUR_MERCHANT_SALT",
    "Currency": "TRY",
    "TestMode": true,
    "Language": "tr",
    "SiteUrl": "https://your-domain.com/",
    "RoutePrefix": "paytr",
    "SuccessUrlPattern": "checkout/success/{correlationId}",
    "FailUrlPattern": "checkout/fail/{correlationId}"
  }
}
```
*Not: `RoutePrefix` webhook (notification) endpoint'i için temel yolu belirler (varsayılan: `/paytr/notification`). `SuccessUrlPattern` ve `FailUrlPattern` ise iFrame ödemesi tamamlandıktan sonra kullanıcıların sizin uygulamanızda yönlendirileceği özel sayfalardır. Kütüphane sürece müdahale etmez, bu dönüş (return) sayfalarını (örneğin MVC Controller veya Minimal API olarak) kendi uygulamanızda sizin yakalamanız ve göstermeniz gerekir.*

## Kullanım

### 1. Ödeme Oluşturma

Bir ödeme başlatmak için, `IPayTrOrderService` arayüzünü controller veya endpoint'inize enjekte edin ve `CreateOrderAndGetPaymentUrlAsync` metodunu çağırın. Kütüphane, sepet verilerini otomatik olarak oluşturur ve güvenlik tokenlarını üretir.

```csharp
app.MapPost("/checkout", async (IPayTrOrderService orderService) =>
{
    var request = new PayTrCreatePaymentRequest
    {
        CorrelationId = Guid.NewGuid(), // Sizin kendi iç sisteminizdeki sipariş ID'niz
        ClientIp = "127.0.0.1",
        PaymentAmount = 250.50m,
        CustomerFullName = "Jane Doe",
        CustomerEmail = "jane@example.com",
        CustomerPhone = "+905555555555",
        CustomerAddress = "Istanbul, Turkey",
        Currency = "TRY",
        BasketItems = new List<PayTrBasketItem>
        {
            new() { Name = "Premium Subscription", Price = 250.50m, Quantity = 1 }
        }
    };

    var response = await orderService.CreateOrderAndGetPaymentUrlAsync(request);

    if (!response.IsSuccess)
    {
        return Results.BadRequest(response.Message);
    }

    // Kullanıcıyı PayTR Ödeme Sayfasına Yönlendirin!
    return Results.Redirect(response.RedirectUrl);
});
```

### 2. Ödeme Sonucunu İşleme (Event Handler)

Durumları manuel olarak kontrol etmek veya karmaşık webhook kodları yazmak yerine, sadece `IPayTrOrderEventHandler` arayüzünü uygulamanız yeterlidir.

Bir ödeme başarılı veya başarısız olduğunda, kütüphane PayTR webhook'unu otomatik olarak yakalar, güvenli bir şekilde doğrular, veritabanını günceller ve handler'ınızı tetikler:

```csharp
using Ali.PayTr.Abstractions.Events;
using Ali.PayTr.Abstractions.Models;

public class MyOrderEventHandler : IPayTrOrderEventHandler
{
    public Task OnPaymentSucceededAsync(Guid correlationId, OrderPayTrNotificationDto notification)
    {
        // İş mantığınızı buraya ekleyin!
        // örn. Kullanıcının premium durumunu güncelleyin, onay e-postası gönderin vb.
        Console.WriteLine($"Order {correlationId} succeeded!");
        return Task.CompletedTask;
    }

    public Task OnPaymentFailedAsync(Guid correlationId, OrderPayTrNotificationDto notification)
    {
        // Ödeme hatasını işleyin...
        Console.WriteLine($"Order {correlationId} failed. Reason: {notification.FailedReasonDescription}");
        return Task.CompletedTask;
    }
}
```
## EF Core Konfigurasyonu

EFCore paketini yukledikten sonra gerekli tablolarin olusmasi icin EF Context'in icerisindeki onModelBuilder metodunu override edip bunu eklemeniz gerekiyor:

```csharp
modelBuilder.ApplyPayTrPaymentModels();
```

## Veritabanı Özelleştirme (EF Core Harici)

Kütüphane, kutudan `Ali.PayTr.EFCore` paketiyle birlikte çıkar. Eğer MongoDB, Dapper veya başka bir ORM kullanmak isterseniz, EF Core paketini tamamen görmezden gelebilirsiniz!

Basitçe `Ali.PayTr.Abstractions` projesinden `IPayTrRepository` arayüzünü uygulayın ve DI container'ınıza kaydedin:

```csharp
public class MyMongoDbPayTrRepository : IPayTrRepository 
{
    // AddOrderAsync, UpdateOrderAsync vb. metodları uygulayın.
}

// Program.cs İçerisinde
builder.Services.AddScoped<IPayTrRepository, MyMongoDbPayTrRepository>();
```

### Entity Framework Core İçin Tablo Yapılandırması

Eğer EF Core paketini (`Ali.PayTr.EFCore`) kullanıyorsanız, `DbContext` sınıfınızın `OnModelCreating` metodunda konfigürasyonları uygulamayı unutmayın:

```csharp
using Ali.PayTr.EFCore.Extensions;

public class AppDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // PayTR tablolarını oluşturmak için bu metodu çağırın!
        modelBuilder.ApplyPayTrPaymentModels();
    }
}
```

Çekirdek (core) kütüphane otomatik olarak kendi repository'nizi bulacak ve tüm işlemler için onu kullanacaktır!

## Ek Servisler

Kütüphane ayrıca işlemleri iade etme (refund) ve ödeme durumunu sorgulama (query) için standart arayüzler sunar:
- `IPayTrRefundService.RefundAsync(...)`
- `IPayTrQueryService.QueryPaymentStatusAsync(...)`

Bunları ihtiyaç duyduğunuz her yerde doğrudan enjekte edebilirsiniz.

---

# 🇬🇧 Ali.PayTr.Payment (English)

A modular, clean-architecture .NET payment integration library for **PayTR**. This library abstracts away the complexities of PayTR integration (hashing, webhook validation, and database tracking), providing developers with an extremely simple plug-and-play experience.

## Features

✅ iFrame Checkout Integration

✅ Webhook / Notification Validation

✅ **Simplest Integration Experience**: Create an order and get a redirect URL with a single method call.

✅ **Automated Webhook Processing**: Automatically handles PayTR's server-to-server notifications (`/paytr/notification`), validates HMAC hashes, and verifies idempotency safely.

✅ **Configurable Routing**: Built-in Minimal APIs with a configurable `RoutePrefix` (e.g., `/api/payments/paytr`).

✅ **Data Persistence Flexibility**: Ships with an Entity Framework Core implementation, but allows full customizability (e.g., MongoDB, Dapper) by simply implementing `IPayTrRepository`.

✅ **Event-Driven Architecture**: Exposes `IPayTrOrderEventHandler` so your business logic is cleanly decoupled from the payment infrastructure.

✅ **Fail-Safe Configuration**: Uses ASP.NET Core `IOptions` with DataAnnotations that validate your configuration immediately at application startup.

## Installation & Registration

Install the required package via NuGet:
```bash
dotnet add package Ali.PayTr
```
*(This meta-package includes Core, EFCore, and AspNetCore. You can also install `Ali.PayTr.Core` standalone if you want to implement your own database persistence.)*

Register the services during your application startup (`Program.cs`):

```csharp
using Ali.PayTr.Core.DependencyInjection;
using Ali.PayTr.EFCore.DependencyInjection;
using Ali.PayTr.AspNetCore.EndpointMapping;

// 1. Add PayTr Core (handles configuration, hashing, clients, and services)
builder.Services.AddPayTrPaymentsCore(builder.Configuration);

// 2. Add the Entity Framework Core Repository (or register your own IPayTrRepository)
builder.Services.AddPayTrPaymentsEFCore<AppDbContext>();

// 3. Register your custom event handler to listen to successful/failed payments
builder.Services.AddPayTrOrderEventHandler<MyOrderEventHandler>();

var app = builder.Build();

// 4. Map the webhooks and return endpoints
app.MapPayTrPaymentEndpoints();

app.Run();
```

## Configuration

The package leverages the standard ASP.NET Core configuration system. You can define these settings in `appsettings.json`, `secrets.json` (User Secrets), or via Environment Variables.

**Important**: Because of the built-in startup validation, if any of the `[Required]` properties are missing from your configuration (even User Secrets or Env Variables), your application will immediately throw an exception on startup to prevent silent failures in production!

Example `appsettings.json`:
```json
{
  "PayTr": {
    "MerchantId": "YOUR_MERCHANT_ID",
    "MerchantKey": "YOUR_MERCHANT_KEY",
    "MerchantSalt": "YOUR_MERCHANT_SALT",
    "Currency": "TRY",
    "TestMode": true,
    "Language": "tr",
    "SiteUrl": "https://your-domain.com/",
    "RoutePrefix": "paytr",
    "SuccessUrlPattern": "checkout/success/{correlationId}",
    "FailUrlPattern": "checkout/fail/{correlationId}"
  }
}
```
*Note: `RoutePrefix` defines the base path for your webhook endpoint (default: `/paytr/notification`). `SuccessUrlPattern` and `FailUrlPattern` define where users are redirected in your own application after the iFrame payment completes. The library does not intercept these, meaning you must implement and handle these return routes (e.g., in your MVC Controllers or Minimal APIs) yourself.*

## Usage

### 1. Creating a Payment

To initiate a payment, simply inject `IPayTrOrderService` into your controller or endpoint and call `CreateOrderAndGetPaymentUrlAsync`. The library automatically builds the basket payload and generates the security tokens.

```csharp
app.MapPost("/checkout", async (IPayTrOrderService orderService) =>
{
    var request = new PayTrCreatePaymentRequest
    {
        CorrelationId = Guid.NewGuid(), // Your internal unique order ID
        ClientIp = "127.0.0.1",
        PaymentAmount = 250.50m,
        CustomerFullName = "Jane Doe",
        CustomerEmail = "jane@example.com",
        CustomerPhone = "+905555555555",
        CustomerAddress = "Istanbul, Turkey",
        Currency = "TRY",
        BasketItems = new List<PayTrBasketItem>
        {
            new() { Name = "Premium Subscription", Price = 250.50m, Quantity = 1 }
        }
    };

    var response = await orderService.CreateOrderAndGetPaymentUrlAsync(request);

    if (!response.IsSuccess)
    {
        return Results.BadRequest(response.Message);
    }

    // Redirect the user to the PayTR Checkout Page!
    return Results.Redirect(response.RedirectUrl);
});
```

### 2. Handling the Payment Result (Event Handler)

Instead of manually checking statuses or writing messy webhook logic, you simply implement the `IPayTrOrderEventHandler` interface.

When a payment succeeds or fails, the library will automatically catch the PayTR webhook, validate it securely, update the database, and trigger your handler:

```csharp
using Ali.PayTr.Abstractions.Events;
using Ali.PayTr.Abstractions.Models;

public class MyOrderEventHandler : IPayTrOrderEventHandler
{
    public Task OnPaymentSucceededAsync(Guid correlationId, OrderPayTrNotificationDto notification)
    {
        // Add your business logic here!
        // e.g., Update user's premium status, send confirmation email, etc.
        Console.WriteLine($"Order {correlationId} succeeded!");
        return Task.CompletedTask;
    }

    public Task OnPaymentFailedAsync(Guid correlationId, OrderPayTrNotificationDto notification)
    {
        // Handle payment failure...
        Console.WriteLine($"Order {correlationId} failed. Reason: {notification.FailedReasonDescription}");
        return Task.CompletedTask;
    }
}
```

## Customizing the Database Persistence (Non-EF Core)

The library ships with `Ali.PayTr.EFCore` out of the box. If you want to use MongoDB, Dapper, or any other ORM, you can completely ignore the EF Core package! 

Simply implement `IPayTrRepository` from the `Ali.PayTr.Abstractions` project and register it in your DI container:

```csharp
public class MyMongoDbPayTrRepository : IPayTrRepository 
{
    // Implement AddOrderAsync, UpdateOrderAsync, etc.
}

// In Program.cs
builder.Services.AddScoped<IPayTrRepository, MyMongoDbPayTrRepository>();
```

### Table Configuration for Entity Framework Core

If you are using the EF Core package (`Ali.PayTr.EFCore`), do not forget to apply the configurations in your `DbContext`'s `OnModelCreating` method:

```csharp
using Ali.PayTr.EFCore.Extensions;

public class AppDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Call this to apply PayTR table configurations!
        modelBuilder.ApplyPayTrPaymentModels();
    }
}
```

The core library will automatically pick up your repository and use it for all operations!

## Additional Services

The library also provides standard interfaces for refunding and querying transaction statuses:
- `IPayTrRefundService.RefundAsync(...)`
- `IPayTrQueryService.QueryPaymentStatusAsync(...)`

Just inject them wherever you need them.
