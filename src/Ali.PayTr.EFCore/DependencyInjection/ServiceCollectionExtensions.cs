using Ali.PayTr.Core.Interfaces;
using Ali.PayTr.EFCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ali.PayTr.EFCore.DependencyInjection;

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
