using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayTr.Payment.Abstractions.Interfaces;
using PayTr.Payment.Abstractions.Options;
using PayTr.Payment.Core.Clients;
using PayTr.Payment.Core.Services;

namespace PayTr.Payment.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPayTrPaymentsCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PayTrOptions>(configuration.GetSection("PayTr")); // Or similar section name

        services.AddHttpClient<IPayTrClient, PayTrClient>();

        services.AddSingleton<IPayTrHashService, PayTrHashService>(); // Singleton is safe for HashService
        services.AddSingleton<PayTrFailReasonService>();
        services.AddScoped<IPayTrOrderService, PayTrOrderService>();
        services.AddScoped<IPayTrNotificationProcessor, PayTrNotificationProcessor>();

        return services;
    }
}
