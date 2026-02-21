using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayTr.Payment.Abstractions.Interfaces;
using PayTr.Payment.Abstractions.Models;
using PayTr.Payment.Abstractions.Options;
using PayTr.Payment.Core.Entities;
using PayTr.Payment.Core.Interfaces;
using PayTr.Payment.Core.Utilities;

namespace PayTr.Payment.Core.Services;

public sealed class PayTrNotificationProcessor : IPayTrNotificationProcessor
{
    // Need IServiceScopeFactory because Notification handling might need fresh scope? 
    // Usually Processor is Scoped, so we can inject Repository directly.
    private readonly IPayTrRepository _repository;
    private readonly IPayTrHashService _hashService;
    private readonly PayTrOptions _options;
    private readonly ILogger<PayTrNotificationProcessor> _logger;
    private readonly PayTrFailReasonService _payTrFailReasonService;

    public PayTrNotificationProcessor(
        IPayTrRepository repository,
        IPayTrHashService hashService,
        IOptions<PayTrOptions> options,
        ILogger<PayTrNotificationProcessor> logger,
        PayTrFailReasonService payTrFailReasonService)
    {
        _repository = repository;
        _hashService = hashService;
        _options = options.Value;
        _logger = logger;
        _payTrFailReasonService = payTrFailReasonService;
    }

    public async Task ProcessNotificationAsync(PayTrNotificationRequest notification, CancellationToken cancellationToken = default)
    {
        // 1. Verify Hash
        var hashStr = notification.MerchantOid + _options.MerchantSalt + notification.Status + notification.TotalAmount;
        var expectedHash = _hashService.CreateTokenHash(hashStr, _options.MerchantKey, ""); // Salt is empty here as per PayTR docs (it's in body)
        var failedReason = _payTrFailReasonService.GetFailedReasonByReasonCode(notification.FailedReasonCode);
        notification.FailedReasonMsg = failedReason.failed_reason_msg;
        notification.FailedReasonDescription = failedReason.description;
        var correlationId = MerchantOidConverter.ToGuid(notification.MerchantOid);
        // PayTR doc quirk: when validating param, salt is appended to body, and CreateTokenHash usually does Body + Salt.
        // My PayTrHashService.CreateTokenHash(str, key, salt) -> Hmac(str + salt).
        // PayTR Doc: Token = Hmac(str + salt).
        // CALLBACK Hash: base64(hmac_sha256(merchant_oid + merchant_salt + status + total_amount, merchant_key))
        // My HashService: Hmac(data + salt).
        // So for callback, data = "merchant_oid + merchant_salt + status + total_amount".
        // And "salt" arg to HashService should be Empty string.

        if (expectedHash != notification.Hash)
        {
            _logger.LogWarning("Invalid Hash for Order: {correlationId}", correlationId);
            // We still assume we might log this? 
            // Requirement: "If hash is invalid: still log notification (optional), do not update order".
            await LogNotificationAsync(notification, "Invalid Hash", null, cancellationToken);
            return;
        }

        // 2. Find Order
        var order = await _repository.GetOrderByCorrelationIdAsync(correlationId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order not found for MerchantOid: {oid}", notification.MerchantOid);
            await LogNotificationAsync(notification, "Order Not Found", null, cancellationToken);
            return;
        }

        // 3. Update Order
        var oldStatus = order.Status;
        var newStatus = notification.Status == "success" ? "Success" : "Failed";

        // Only update if not already final? 
        // PayTR might send multiple notifications.
        if (oldStatus != "Success")
        {
            order.Status = newStatus;
            order.UpdatedDate = DateTimeOffset.UtcNow;
            await _repository.UpdateOrderAsync(order, cancellationToken);

            await _repository.AddLogAsync(new PayTrOrderLogHistory
            {
                PayTrOrderId = order.Id,
                Message = notification.Status == "success" ? "Payment Successful" : $"Payment Failed: {notification.FailedReasonMsg}",
                OldStatus = oldStatus,
                NewStatus = newStatus,
                CreatedDate = DateTimeOffset.UtcNow
            }, cancellationToken);
        }

        // 4. Log Notification
        await LogNotificationAsync(notification, null, order.Id, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task LogNotificationAsync(PayTrNotificationRequest notification, string? failedReason, Guid? orderId, CancellationToken ct)
    {
        var history = new PayTrNotificationHistory
        {
            PayTrOrderId = orderId,
            MerchantOid = notification.MerchantOid,
            Status = notification.Status,
            RawBody = SerializeRawForm(notification),
            Hash = notification.Hash,
            FailedReason = failedReason ?? notification.FailedReasonMsg,
            CreatedDate = DateTimeOffset.UtcNow
        };
        await _repository.AddNotificationAsync(history, ct);
        // Note: SaveChanges is called by caller or here? Caller calls it. 
        // But if we returned early (hash fail), we need to save.
        if (failedReason != null)
        {
            await _repository.SaveChangesAsync(ct);
        }
    }

    private string SerializeRawForm(PayTrNotificationRequest request)
    {
        // simplistic serialization
        return $"oid={request.MerchantOid}|status={request.Status}|amt={request.TotalAmount}|hash={request.Hash}|correlationId={MerchantOidConverter.ToGuid(request.MerchantOid)}";
    }
}
