using Ali.PayTr.Abstractions.Enums;
using Ali.PayTr.Abstractions.Events;
using Ali.PayTr.Abstractions.Interfaces;
using Ali.PayTr.Abstractions.Models;
using Ali.PayTr.Abstractions.Options;
using Ali.PayTr.Core.Entities;
using Ali.PayTr.Core.Interfaces;
using Ali.PayTr.Core.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text.Json;

namespace Ali.PayTr.Core.Services;

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

    public async Task<PayTrNotificationVerifyResult> ProcessNotificationAsync(PayTrNotificationRequest notification, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.Parse(notification.MerchantOid);

        var hashStr = notification.MerchantOid + _options.MerchantSalt + notification.Status + notification.TotalAmount;
        var expectedHash = _hashService.CreateTokenHash(hashStr, _options.MerchantKey, "");
        //if (expectedHash != notification.Hash)
        //basically we should not use simple string equality for hash comparison to prevent timing attacks.
        //Instead, we can use a method that compares the hashes in constant time.
        if (!CryptographicOperations.FixedTimeEquals(left: Convert.FromBase64String(expectedHash), right: Convert.FromBase64String(notification.Hash)))
        {
            _logger.LogWarning("Invalid Hash for Order: {correlationId}", correlationId);
            await LogNotificationAsync(notification, "Invalid Hash", null, cancellationToken);

            await _eventDispatcher.DispatchFailureAsync(correlationId, CreateSystemFailureNotification(
                notification,
                failedReasonCode: "SYSTEM_HASH_MISMATCH",
                failedReasonMessage: "Notification hash validation failed. Expected hash and provided hash are not equal.",
                failedReasonDescription: "The callback payload could not be trusted because the cryptographic hash did not match."), cancellationToken);

            return new PayTrNotificationVerifyResult
            {
                IsVerificationSuccessful = false,
                ExpectedHash = expectedHash,
                ReceivedHash = notification.Hash
            };
        }
        if (notification.IsSuccess)
        {
            var failedReason = _payTrFailReasonService.GetFailedReasonByReasonCode(notification.FailedReasonCode);
            notification.FailedReasonMsg = failedReason.failed_reason_msg;
            notification.FailedReasonDescription = failedReason.description;
        }
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

            return new PayTrNotificationVerifyResult
            {
                ReceivedHash = notification.Hash,
                ExpectedHash = expectedHash,
                FailureReason = "Order Not Found",
                IsVerificationSuccessful = true
            };
        }

        var oldStatus = order.Status;
        var newStatus = notification.IsSuccess ? OrderStatus.CompletedWithSuccess.ToString() : OrderStatus.CompletedWithFail.ToString();

        // PayTR can send multiple notifications for the same merchant_oid.
        // We should finalize once and notify client integrations only once.
        var wasFinalized = oldStatus == OrderStatus.CompletedWithSuccess.ToString() || oldStatus == OrderStatus.CompletedWithFail.ToString();
        if (!wasFinalized)
        {
            order.Status = newStatus;
            order.UpdatedDate = DateTimeOffset.UtcNow;
            await _repository.UpdateOrderAsync(order, cancellationToken);

            await _repository.AddLogAsync(new PayTrOrderLogHistory
            {
                PayTrOrderId = order.Id,
                Message = notification.IsSuccess ? "Payment Successful" : $"Payment Failed: {notification.FailedReasonMsg}",
                OldStatus = oldStatus,
                NewStatus = newStatus,
                CreatedDate = DateTimeOffset.UtcNow
            }, cancellationToken);
        }

        await LogNotificationAsync(notification, null, order.Id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        if (wasFinalized)
        {
            return new PayTrNotificationVerifyResult
            {
                ReceivedHash = notification.Hash,
                ExpectedHash = expectedHash,
                FailureReason = "Order was already finalized earlier",
                IsVerificationSuccessful = true
            };
        }
        if (notification.IsSuccess)
        {
            await _eventDispatcher.DispatchSuccessAsync(correlationId, CreateSuccessNotification(notification), cancellationToken);
            return new PayTrNotificationVerifyResult
            {
                ReceivedHash = notification.Hash,
                ExpectedHash = expectedHash,
                FailureReason = "Notification processing was successful and passed",
                IsVerificationSuccessful = true
            };
        }

        await _eventDispatcher.DispatchFailureAsync(correlationId, CreatePayTrFailureNotification(notification), cancellationToken);

        return new PayTrNotificationVerifyResult
        {
            ReceivedHash = notification.Hash,
            ExpectedHash = expectedHash,
            FailureReason = "Notification was not success",
            IsVerificationSuccessful = true
        };
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
            MerchantId = _options.MerchantId.ToString(),
            RawNotificationBody = request.RawFormAsJson,
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
            IsSystemError = true,
            RawNotificationBody = request.RawFormAsJson,
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
            CreatedDate = DateTimeOffset.UtcNow,
        };

        await _repository.AddNotificationAsync(history, ct);

        if (failedReason != null)
        {
            await _repository.SaveChangesAsync(ct);
        }
    }

    private string SerializeRawForm(PayTrNotificationRequest request)
    {
        return JsonSerializer.Serialize(request);
    }
}
