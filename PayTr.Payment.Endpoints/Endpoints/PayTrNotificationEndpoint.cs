using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PayTr.Payment.Abstractions.Interfaces;
using PayTr.Payment.Abstractions.Models;

namespace PayTr.Payment.AspNetCore.Endpoints;

internal static class PayTrNotificationEndpoint
{
    internal static async Task<IResult> HandleAsync(
        IHttpContextAccessor httpContextAccessor,
        IPayTrNotificationProcessor processor,
        ILoggerFactory loggerFactory
        )
    {

        //eventDispatcher.Dispatch(foo);
        var logger = loggerFactory.CreateLogger("PayTrNotificationEndpoint");

        if (!httpContextAccessor.HttpContext!.Request.HasFormContentType)
        {
            logger.LogError("Invalid Content-Type. Expected application/x-www-form-urlencoded.");
            return Results.BadRequest("Invalid Content-Type");
        }

        var form = await httpContextAccessor.HttpContext.Request.ReadFormAsync();

        // Map form to NotificationRequest
        // PayTR sends fields: merchant_oid, status, total_amount, hash, failed_reason_code, failed_reason_msg, test_mode, payment_type, currency, net_amount, etc.
        var failedReasonCode = int.Parse(form["failed_reason_code"].ToString());
        var notification = new PayTrNotificationRequest
        {
            MerchantOid = form["merchant_oid"].ToString(),
            Status = form["status"].ToString(),
            TotalAmount = form["total_amount"].ToString(),
            Hash = form["hash"].ToString(),
            FailedReasonCode = failedReasonCode,
            FailedReasonMsg = form["failed_reason_msg"].ToString(),
            PaymentType = form["payment_type"].ToString(),
            Currency = form["currency"].ToString(),
            TestMode = form["test_mode"].ToString(),
            RawForm = form.ToDictionary(k => k.Key, v => v.Value.ToString())
        };

        await processor.ProcessNotificationAsync(notification);

        return Results.Ok("OK");
    }
}
