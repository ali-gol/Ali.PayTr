using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayTr.Payment.Abstractions.Events;
using PayTr.Payment.Abstractions.Interfaces;
using PayTr.Payment.Abstractions.Models;
using PayTr.Payment.Abstractions.Options;
using PayTr.Payment.Core.Entities;
using PayTr.Payment.Core.Interfaces;
using PayTr.Payment.Core.Utilities;

namespace PayTr.Payment.Core.Services;

public sealed class PayTrNotificationProcessor : IPayTrNotificationProcessor
{
    private readonly IPayTrRepository _repository;
    private readonly IPayTrHashService _hashService;
    private readonly PayTrOptions _options;
    private readonly ILogger<PayTrNotificationProcessor> _logger;
    private readonly PayTrFailReasonService _payTrFailReasonService;
    private readonly IPayTrOrderEventDispatcher _eventDispatcher;

    public PayTrNotificationProcessor(
        IPayTrRepository repository,
        IPayTrHashService hashService,
        IOptions<PayTrOptions> options,
        ILogger<PayTrNotificationProcessor> logger,
        PayTrFailReasonService payTrFailReasonService,
        IPayTrOrderEventDispatcher eventDispatcher)
    {
        _repository = repository;
        _hashService = hashService;
        _options = options.Value;
        _logger = logger;
        _payTrFailReasonService = payTrFailReasonService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task ProcessNotificationAsync(PayTrNotificationRequest notification, CancellationToken cancellationToken = default)
    {
        var correlationId = MerchantOidConverter.ToGuid(notification.MerchantOid);

        var hashStr = notification.MerchantOid + _options.MerchantSalt + notification.Status + notification.TotalAmount;
        var expectedHash = _hashService.CreateTokenHash(hashStr, _options.MerchantKey, "");

        if (expectedHash != notification.Hash)
        {
            _logger.LogWarning("Invalid Hash for Order: {correlationId}", correlationId);
            await LogNotificationAsync(notification, "Invalid Hash", null, cancellationToken);

            await _eventDispatcher.DispatchFailureAsync(correlationId, CreateSystemFailureNotification(
                notification,
                failedReasonCode: "SYSTEM_HASH_MISMATCH",
                failedReasonMessage: "Notification hash validation failed. Expected hash and provided hash are not equal.",
                failedReasonDescription: "The callback payload could not be trusted because the cryptographic hash did not match."), cancellationToken);

            return;
        }

        var failedReason = _payTrFailReasonService.GetFailedReasonByReasonCode(notification.FailedReasonCode);
        notification.FailedReasonMsg = failedReason.failed_reason_msg;
        notification.FailedReasonDescription = failedReason.description;

        var order = await _repository.GetOrderByCorrelationIdAsync(correlationId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order not found for MerchantOid: {oid}", notification.MerchantOid);
            await LogNotificationAsync(notification, "Order Not Found", null, cancellationToken);

            await _eventDispatcher.DispatchFailureAsync(correlationId, CreateSystemFailureNotification(
                notification,
                failedReasonCode: "SYSTEM_ORDER_NOT_FOUND",
                failedReasonMessage: "Order could not be found by correlation id.",
                failedReasonDescription: "Notification is valid but no corresponding order exists in the database."), cancellationToken);

            return;
        }

        var oldStatus = order.Status;
        var isSuccessStatus = string.Equals(notification.Status, "success", StringComparison.OrdinalIgnoreCase);
        var newStatus = isSuccessStatus ? "Success" : "Failed";

        var wasFinalized = oldStatus is "Success" or "Failed";
        if (!wasFinalized)
        {
            order.Status = newStatus;
            order.UpdatedDate = DateTimeOffset.UtcNow;
            await _repository.UpdateOrderAsync(order, cancellationToken);

            await _repository.AddLogAsync(new PayTrOrderLogHistory
            {
                PayTrOrderId = order.Id,
                Message = isSuccessStatus ? "Payment Successful" : $"Payment Failed: {notification.FailedReasonMsg}",
                OldStatus = oldStatus,
                NewStatus = newStatus,
                CreatedDate = DateTimeOffset.UtcNow
            }, cancellationToken);
        }

        await LogNotificationAsync(notification, null, order.Id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        if (!wasFinalized)
        {
            if (isSuccessStatus)
            {
                await _eventDispatcher.DispatchSuccessAsync(correlationId, CreateSuccessNotification(notification), cancellationToken);
                return;
            }

            await _eventDispatcher.DispatchFailureAsync(correlationId, CreatePayTrFailureNotification(notification), cancellationToken);
        }
    }

    private PayTrPaymentSuccessNotification CreateSuccessNotification(PayTrNotificationRequest request)
    {
        return new PayTrPaymentSuccessNotification
        {
            MerchantOid = request.MerchantOid,
            Status = request.Status,
            TotalAmount = request.TotalAmount,
            Hash = request.Hash,
            PaymentType = request.PaymentType,
            Currency = request.Currency,
            TestMode = request.TestMode,
            MerchantId = _options.MerchantId.ToString()
        };
    }

    private PayTrPaymentFailedNotification CreatePayTrFailureNotification(PayTrNotificationRequest request)
    {
        return new PayTrPaymentFailedNotification
        {
            MerchantOid = request.MerchantOid,
            Status = request.Status,
            TotalAmount = request.TotalAmount,
            Hash = request.Hash,
            PaymentType = request.PaymentType,
            Currency = request.Currency,
            TestMode = request.TestMode,
            MerchantId = _options.MerchantId.ToString(),
            FailedReasonCode = request.FailedReasonCode.ToString(),
            FailedReasonMessage = request.FailedReasonMsg,
            FailedReasonDescription = request.FailedReasonDescription,
            IsSystemError = false
        };
    }

    private PayTrPaymentFailedNotification CreateSystemFailureNotification(
        PayTrNotificationRequest request,
        string failedReasonCode,
        string failedReasonMessage,
        string failedReasonDescription)
    {
        return new PayTrPaymentFailedNotification
        {
            MerchantOid = request.MerchantOid,
            Status = request.Status,
            TotalAmount = request.TotalAmount,
            Hash = request.Hash,
            PaymentType = request.PaymentType,
            Currency = request.Currency,
            TestMode = request.TestMode,
            MerchantId = _options.MerchantId.ToString(),
            FailedReasonCode = failedReasonCode,
            FailedReasonMessage = failedReasonMessage,
            FailedReasonDescription = failedReasonDescription,
            IsSystemError = true
        };
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

        if (failedReason != null)
        {
            await _repository.SaveChangesAsync(ct);
        }
    }

    private string SerializeRawForm(PayTrNotificationRequest request)
    {
        return $"oid={request.MerchantOid}|status={request.Status}|amt={request.TotalAmount}|hash={request.Hash}|correlationId={MerchantOidConverter.ToGuid(request.MerchantOid)}";
    }
}
