using Ali.PayTr.Abstractions.Events;
using Ali.PayTr.Abstractions.Interfaces;
using Ali.PayTr.Abstractions.Options;
using Ali.PayTr.Core.Clients;
using Ali.PayTr.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ali.PayTr.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPayTrPaymentsCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PayTrOptions>()
            .Bind(configuration.GetSection("PayTr"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IPayTrClient, PayTrClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PayTrOptions>>().Value;
            client.BaseAddress = new Uri(options.ApiBaseUrl);
        });

        services.AddSingleton<IPayTrHashService, PayTrHashService>();
        services.AddSingleton<PayTrFailReasonService>();
        services.AddScoped<IPayTrOrderService, PayTrOrderService>();
        services.AddScoped<IPayTrNotificationProcessor, PayTrNotificationProcessor>();
        services.AddScoped<IPayTrOrderEventDispatcher, PayTrOrderEventDispatcher>();
        
        services.AddHttpClient<IPayTrRefundService, PayTrRefundService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PayTrOptions>>().Value;
            client.BaseAddress = new Uri(options.ApiBaseUrl);
        });

        services.AddHttpClient<IPayTrQueryService, PayTrQueryService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PayTrOptions>>().Value;
            client.BaseAddress = new Uri(options.ApiBaseUrl);
        });

        return services;
    }

    public static IServiceCollection AddPayTrOrderEventHandler<THandler>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where THandler : class, IPayTrOrderEventHandler
    {
        services.Add(new ServiceDescriptor(typeof(IPayTrOrderEventHandler), typeof(THandler), lifetime));
        return services;
    }
}
