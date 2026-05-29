using Ali.PayTr.Abstractions.Interfaces;
using Ali.PayTr.Abstractions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ali.PayTr.AspNetCore.Endpoints;

internal static class PayTrNotificationEndpoint
{
    internal static async Task<IResult> HandleAsync(
        IHttpContextAccessor httpContextAccessor,
        IPayTrNotificationProcessor processor,
        ILoggerFactory loggerFactory,
        IPayTrHashService hashService,
        Microsoft.Extensions.Options.IOptions<Ali.PayTr.Abstractions.Options.PayTrOptions> options
        )
    {
        var logger = loggerFactory.CreateLogger(nameof(PayTrNotificationEndpoint));

        if (!httpContextAccessor.HttpContext!.Request.HasFormContentType)
        {
            logger.LogError("Invalid Content-Type. Expected application/x-www-form-urlencoded.");
            return Results.BadRequest("Invalid Content-Type");
        }

        var form = await httpContextAccessor.HttpContext.Request.ReadFormAsync();
        var formValues = form as IEnumerable<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>;

        var notification = new PayTrNotificationRequest
        {
            MerchantOid = form["merchant_oid"].ToString(),
            Status = form["status"].ToString(),
            TotalAmount = form["total_amount"].ToString(),
            Hash = form["hash"].ToString(),
            //FailedReasonCode = failedReasonCode,
            FailedReasonMsg = form["failed_reason_msg"].ToString(),
            PaymentType = form["payment_type"].ToString(),
            Currency = form["currency"].ToString(),
            TestMode = form["test_mode"].ToString(),
            RawFormAsJson = JsonSerializer.Serialize(form.ToDictionary(k => k.Key, v => v.Value[0]))
        };

        var processResult = await processor.ProcessNotificationAsync(notification);
        if (!processResult.IsVerificationSuccessful)
        {
            logger.LogError("Failed to process PayTr notification. Reason: {Reason}", processResult);
            return Results.BadRequest("Failed to process notification");
        }
        return Results.Ok("OK");
    }
}
