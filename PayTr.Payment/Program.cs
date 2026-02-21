using Microsoft.EntityFrameworkCore;
using PayTr.Payment.Core.DependencyInjection;
using PayTr.Payment.EFCore.DependencyInjection;
using PayTr.Payment.Endpoints.DependencyInjection;
using PayTr.Payment.Endpoints.Routing;
using PayTr.Payment.Sample;
using PayTr.Payment.Abstractions.Interfaces; // For testing
using PayTr.Payment.Abstractions.Models; // For testing
using Microsoft.AspNetCore.Mvc; // For testing

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("PayTrDb"));

// PayTR Config
// Ensure "PayTr" section exists in appsettings or we mock it.
// We'll add in-memory config for testing.
var payTrConfig = new Dictionary<string, string>
{
    {"PayTr:MerchantId", "TEST_MERCHANT_ID"},
    {"PayTr:MerchantKey", "TEST_KEY"},
    {"PayTr:MerchantSalt", "TEST_SALT"},
    {"PayTr:TestMode", "true"}
};
var configBuilder = new ConfigurationBuilder()
    .AddInMemoryCollection(payTrConfig);
var config = configBuilder.Build();

// We need to merge this config with builder.Configuration or just pass it?
// The extension method takes IConfiguration. 
// Ideally we append to builder.Configuration.
builder.Configuration.AddInMemoryCollection(payTrConfig!);

builder.Services.AddPayTrPaymentsCore(builder.Configuration);
builder.Services.AddPayTrPaymentsEFCore<AppDbContext>();
builder.Services.AddPayTrPaymentsEndpoints();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapPayTrPaymentEndpoints();

// Test Endpoint to Create Payment
app.MapPost("/test/create-payment", async (
    [FromServices] IPayTrOrderService orderService,
    [FromQuery] decimal amount) =>
{
    var request = new PayTrCreatePaymentRequest
    {
        MerchantOid = Guid.NewGuid().ToString(), // Unique
        PaymentAmount = amount,
        Email = "test@test.com",
        UserName = "Test User",
        UserAddress = "Test Address",
        UserPhone = "5555555555",
        Currency = "TL",
        ClientIp = "127.0.0.1",
        BasketItems = new List<PayTrBasketItem>
        {
            new PayTrBasketItem { Name = "Product 1", Price = amount.ToString(), Quantity = 1 }
        },
        OkUrl = "https://localhost:5001/paytr/return",
        FailUrl = "https://localhost:5001/paytr/return"
    };
    
    // This will fail because HttpClient will try to hit PayTR API with Fake Keys.
    // But we check if it reaches that point (meaning DB saved).
    var result = await orderService.CreateOrderAsync(request);
    
    return result;
});

// Create DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
