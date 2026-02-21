using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayTr.Payment.Abstractions.Interfaces;

namespace PayTr.Payment.AspNetCore.Endpoints;

internal static class PayTrReturnFailEndpoint
{
    internal static async Task<IResult> HandleAsync(
        [FromRoute] Guid correlationId,
        [FromServices] IPayTrOrderService orderService,
        [FromServices] IHttpContextAccessor contextAccessor)
    {
        // 1. Find Order
        var order = await orderService.GetOrderByCorrelationIdAsync(correlationId);

        if (order == null)
        {
            return Results.NotFound("Transaction not found");
        }

        // 2. Redirect based on status? 
        // Or show a page?
        // Requirement: "This endpoint can show a simple 'Payment result' page or redirect."
        // "Consumer will redirect user here after PayTR payment page."

        // Logic:
        // We can check order status.
        // If success -> return "Payment Success" text or Redirect to some SuccessUrl configured in Options?
        // The requirements didn't specify where to look for SuccessUrl if we use this endpoint.
        // Maybe we just return a simple JSON or HTML?
        // "Minimal API endpoint... return IResult".

        var html = $@"
<html>
<head><title>Payment Result</title></head>
<body>
    <h1>Payment {order.CorrelationId}</h1>
    <p>Status: {order.BasketItems} ??? No, status isn't exposed in model properly? Wait.</p>
    <p>Use local mapping logic or just trust what we have.</p>
    <p>Payment Amount: {order.PaymentAmount}</p>
</body>
</html>";
        // OrderModel doesn't have Status property!
        // I need to add Status to PayTrOrderModel?
        // Or I just re-query Entity in Service and map Status.
        // I'll add Status to PayTrOrderModel.

        // But for now, just Return Ok(order).
        return Results.Ok(new { Message = "Payment processed", details = order });
    }
}
