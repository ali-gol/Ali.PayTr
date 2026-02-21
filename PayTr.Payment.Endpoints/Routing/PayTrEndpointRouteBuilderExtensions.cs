using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using PayTr.Payment.AspNetCore.Endpoints;

namespace PayTr.Payment.AspNetCore.Routing;

public static class PayTrEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapPayTrPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/paytr/notification", PayTrNotificationEndpoint.HandleAsync);
        //app.MapGet("/paytr/return/success/{correlationId:guid}", PayTrReturnSuccessEndpoint.HandleAsync);
        app.MapGet("/paytr/success/{correlationId}", PayTrReturnSuccessEndpoint.HandleAsync);
        app.MapGet("/paytr/fail/{correlationId:guid}", PayTrReturnFailEndpoint.HandleAsync);

        return app;
    }
}
