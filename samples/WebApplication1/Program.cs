using Ali.PayTr.AspNetCore.EndpointMapping;
using Ali.PayTr.Core.DependencyInjection;
using Ali.PayTr.EFCore.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using WebApplication1.EventHandlers;
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AliDbContext>(options =>
{
    options.UseInMemoryDatabase("AliDb");
});

builder.Services.AddControllersWithViews();

builder.Services.AddPayTrPaymentsCore(builder.Configuration);
builder.Services.AddPayTrPaymentsEFCore<AliDbContext>();
builder.Services.AddPayTrOrderEventHandler<AliOrderEventHandler>();
var app = builder.Build();

app.UseHttpsRedirection();
app.MapPayTrPaymentEndpoints();
app.UseRouting();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
