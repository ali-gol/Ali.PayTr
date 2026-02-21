using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PayTr.Payment.Core.Interfaces;
using PayTr.Payment.EFCore.Repositories;

namespace PayTr.Payment.EFCore.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPayTrPaymentsEFCore<TContext>(this IServiceCollection services) 
        where TContext : DbContext
    {
        // Register repository using the provided DbContext type
        services.AddScoped<IPayTrRepository, PayTrRepository<TContext>>();

        return services;
    }
}
