using Microsoft.EntityFrameworkCore;
using PayTr.Payment.AspNetCore.Routing;
using PayTr.Payment.Core.DependencyInjection;
using PayTr.Payment.EFCore.DependencyInjection;
using WebApplication1.Models;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
// Add services to the container.
builder.Services.AddDbContext<AliDbContext>(options =>
{
    options.UseInMemoryDatabase("AlininDb");

});
builder.Services.AddControllersWithViews();
builder.Services.AddPayTrPaymentsCore(builder.Configuration);
builder.Services.AddPayTrPaymentsEFCore<AliDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapPayTrPaymentEndpoints();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
