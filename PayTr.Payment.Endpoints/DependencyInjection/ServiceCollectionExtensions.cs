using Microsoft.Extensions.DependencyInjection;

namespace PayTr.Payment.AspNetCore.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPayTrPaymentsEndpoints(this IServiceCollection services)
    {
        // Endpoints are static, so no registration needed for them.
        // Services they depend on (IPayTrOrderService, IPayTrNotificationProcessor) should be registered by Core.
        
        return services;
    }
}
