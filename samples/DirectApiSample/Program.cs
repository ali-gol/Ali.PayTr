using Ali.PayTr.AspNetCore.EndpointMapping;
using Ali.PayTr.Core.DependencyInjection;
using Ali.PayTr.EFCore.DependencyInjection;
using DirectApiSample.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configure PayTR
builder.Services.AddPayTrPaymentsCore(builder.Configuration);
builder.Services.AddPayTrPaymentsEFCore<AppDbContext>();
builder.Services.AddPayTrAspNetCore();
builder.Services.AddPayTrDirectApi(); // Enables Direct API

var app = builder.Build();

// Migrate DB on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Payment}/{action=Index}/{id?}")
    .WithStaticAssets();

// Map PayTR Webhook (/paytr/notification)
app.MapPayTrPaymentEndpoints();

app.Run();
