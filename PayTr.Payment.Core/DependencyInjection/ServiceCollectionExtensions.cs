using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PayTr.Payment.Abstractions.Events;
using PayTr.Payment.Abstractions.Interfaces;
using PayTr.Payment.Abstractions.Options;
using PayTr.Payment.Core.Clients;
using PayTr.Payment.Core.Services;

namespace PayTr.Payment.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPayTrPaymentsCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PayTrOptions>(configuration.GetSection("PayTr"));

        services.AddHttpClient<IPayTrClient, PayTrClient>();

        services.AddSingleton<IPayTrHashService, PayTrHashService>();
        services.AddSingleton<PayTrFailReasonService>();
        services.AddScoped<IPayTrOrderService, PayTrOrderService>();
        services.AddScoped<IPayTrNotificationProcessor, PayTrNotificationProcessor>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPayTrOrderEventHandler, NullPayTrOrderEventHandler>());

        return services;
    }

    public static IServiceCollection AddPayTrOrderEventHandler<THandler>(this IServiceCollection services)
        where THandler : class, IPayTrOrderEventHandler
    {
        services.AddSingleton<IPayTrOrderEventHandler, THandler>();
        return services;
    }
}
