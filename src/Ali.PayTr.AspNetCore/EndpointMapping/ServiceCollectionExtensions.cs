using Ali.PayTr.AspNetCore.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ali.PayTr.Abstractions.Options;
using Ali.PayTr.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Ali.PayTr.AspNetCore.EndpointMapping;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPayTrAspNetCore(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IPayTrClientIpAccessor, Ali.PayTr.AspNetCore.Services.PayTrClientIpAccessor>();
        return services;
    }

    public static IEndpointRouteBuilder MapPayTrPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var options = app.ServiceProvider.GetRequiredService<IOptions<PayTrOptions>>().Value;
        var prefix = string.IsNullOrWhiteSpace(options.RoutePrefix) ? "paytr" : options.RoutePrefix.Trim('/');

        var group = app.MapGroup($"/{prefix}")
            .MapEndponts(options)
            .WithTags("PayTr Return Handlers");

        return app;
    }

    private static RouteGroupBuilder MapEndponts(this RouteGroupBuilder group, PayTrOptions options)
    {
        group.MapPost("/notification", PayTrNotificationEndpoint.HandleAsync);
        return group;
    }
}
