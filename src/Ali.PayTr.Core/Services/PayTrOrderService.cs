using Ali.PayTr.Abstractions.Enums;
using Ali.PayTr.Abstractions.Interfaces;
using Ali.PayTr.Abstractions.Models;
using Ali.PayTr.Core.Entities;
using Ali.PayTr.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ali.PayTr.Core.Services;

public sealed class PayTrOrderService : IPayTrOrderService
{
    private readonly IPayTrClient _client;
    private readonly IPayTrRepository _repository;
    private readonly ILogger<PayTrOrderService> _logger;

    public PayTrOrderService(
        IPayTrClient client,
        IPayTrRepository repository,
        ILogger<PayTrOrderService> logger)
    {
        _client = client;
        _repository = repository;
        _logger = logger;
    }

    public async Task<PayTrCreatePaymentResponse> CreateOrderAndGetPaymentUrlAsync(PayTrCreatePaymentRequest request, CancellationToken cancellationToken = default)
    {

        var order = new PayTrOrder
        {
            Id = Guid.NewGuid(),
            TotalAmount = request.PaymentAmount,
            CreatedDate = DateTimeOffset.UtcNow,
            Status = OrderStatus.Created.ToString(),
            Currency = request.Currency,
            CustomerEmail = request.CustomerEmail,
            CustomerAddress = request.CustomerAddress,
            CustomerFullName = request.CustomerFullName,
            CustomerPhone = request.CustomerPhone,
            ClientIp = request.ClientIp,
            BasketJson = JsonSerializer.Serialize(request.BasketItems),
            CorrelationId = request.CorrelationId,
            InstallmentCount = request.InstallmentCount
        };

        await _repository.AddOrderAsync(order, cancellationToken);
        await _repository.AddLogAsync(new PayTrOrderLogHistory
        {
            PayTrOrderId = order.Id,
            CorrelationId = order.CorrelationId,
            Message = "Order created locally",
            CreatedDate = DateTimeOffset.UtcNow,
            NewStatus = OrderStatus.Created.ToString()
        }, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        _logger.LogTrace($"Order created locally: {request.CorrelationId}");

        // 2. Call PayTR API
        // The Consumer sets them in `PayTrCreatePaymentRequest`. 
        var response = await _client.CreatePaymentAsync(request, cancellationToken);
        if (response.IsSuccess)
        {
            await _repository.AddLogAsync(new PayTrOrderLogHistory
            {
                PayTrOrderId = order.Id,
                CorrelationId = order.CorrelationId,
                Message = $"PayTR API Token Success. Token: {response.Token}",
                CreatedDate = DateTimeOffset.UtcNow,
                OldStatus = OrderStatus.Created.ToString(),
                NewStatus = OrderStatus.Created.ToString()
            }, cancellationToken);
        }
        else
        {
            order.Status = OrderStatus.CompletedWithFail.ToString();
            await _repository.AddLogAsync(new PayTrOrderLogHistory
            {
                PayTrOrderId = order.Id,
                CorrelationId = order.CorrelationId,
                Message = $"PayTR API Failed: {response.Message}",
                CreatedDate = DateTimeOffset.UtcNow,
                OldStatus = OrderStatus.Created.ToString(),
                NewStatus = OrderStatus.CompletedWithFail.ToString()
            }, cancellationToken);
        }
        await _repository.SaveChangesAsync(cancellationToken);
        return response;
    }

    public async Task<PayTrOrderModel?> GetOrderByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetOrderByCorrelationIdAsync(correlationId, cancellationToken);
        return MapToModel(entity);
    }

    private PayTrOrderModel? MapToModel(PayTrOrder? entity)
    {
        if (entity == null)
            return null;

        return new PayTrOrderModel
        {
            BasketItems = JsonSerializer.Deserialize<List<PayTrBasketItem>>(entity.BasketJson!) ?? new(),
            CorrelationId = entity.CorrelationId,
            CustomerEmail = entity.CustomerEmail ?? string.Empty,
            PaymentAmount = entity.TotalAmount,
            InstallmentCount = entity.InstallmentCount,
            CustomerPhone = entity.CustomerPhone ?? string.Empty,
            CustomerAddress = entity.CustomerAddress ?? string.Empty,
            CustomerFullName = entity.CustomerFullName ?? string.Empty,
            PaymentType = entity.PaymentType
        };
    }
}
