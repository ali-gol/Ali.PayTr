using Microsoft.Extensions.Logging;
using PayTr.Payment.Abstractions.Interfaces;
using PayTr.Payment.Abstractions.Models;
using PayTr.Payment.Core.Entities;
using PayTr.Payment.Core.Interfaces;
using PayTr.Payment.Core.Utilities;
using System.Text.Json;

namespace PayTr.Payment.Core.Services;

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

    public async Task<PayTrCreatePaymentResponse> CreateOrderAsync(PayTrCreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Create Order Entity
        var order = new PayTrOrder
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            TotalAmount = request.PaymentAmount,
            CreatedDate = DateTimeOffset.UtcNow,
            Status = "Pending",
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

        // If MerchantOid is not provided, maybe generate one? 
        // Request model has "required" implicitly by being non-nullable string in C# (if enabled). 
        // Assuming consumer provides unique MerchantOid.

        await _repository.AddOrderAsync(order, cancellationToken);
        await _repository.AddLogAsync(new PayTrOrderLogHistory
        {
            PayTrOrderId = order.Id,
            CorrelationId = order.CorrelationId,
            Message = "Order created locally",
            CreatedDate = DateTimeOffset.UtcNow,
            NewStatus = "Pending"
        }, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        _logger.LogTrace($"Order created locally: {request.CorrelationId}");
        // 2. Call PayTR API

        // Use case: The package exposes correct URLs?
        // "This is the only public entry... PayTrReturnEndpoint".

        // I will assume for now we don't force override URLs in Service. 
        // The Consumer sets them in `PayTrCreatePaymentRequest`. 
        // if they want to use our ReturnEndpoint, they set `OkUrl = "https://mysite.com/paytr/return/..."`.

        var response = await _client.CreatePaymentAsync(request, cancellationToken);

        if (!response.IsSuccess)
        {
            order.Status = "Failed";
            // Record failure
            await _repository.AddLogAsync(new PayTrOrderLogHistory
            {
                PayTrOrderId = order.Id,
                CorrelationId = order.CorrelationId,
                Message = $"PayTR API Failed: {response.Message}",
                CreatedDate = DateTimeOffset.UtcNow,
                OldStatus = "Pending",
                NewStatus = "Failed"
            }, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }
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
            BasketItems = JsonSerializer.Deserialize<List<PayTrBasketItem>>(entity.BasketJson),
            CorrelationId = entity.CorrelationId,
            CustomerEmail = entity.CustomerEmail,
            PaymentAmount = entity.TotalAmount,
            InstallmentCount = entity.InstallmentCount,
            CustomerPhone = entity.CustomerPhone,
            CustomerAddress = entity.CustomerAddress,
            CustomerFullName = entity.CustomerFullName,
            PaymentType = entity.PaymentType
            // Need more manual mapping if PayTrOrderModel requires more
            // PayTrOrderModel has: 
            // Guid CorrelationId, string Email, int PaymentAmount, List<PayTrBasketItem> BasketItems,
            // string CustomerFullName, CustomerAddress, CustomerPhone, PaymentType.
            // My Entity does not store Address, Phone, Name separately (maybe in basket or separate fields?).
            // Entity has fields defined in `PayTrOrder.cs`: UserId, MerchantOid, Status, TotalAmount, CreatedDate, UpdatedDate, BasketJson, Currency, Email, ClientIp, CorrelationId.
            // Doesn't have Address/Name/Phone.
            // So I can't map them back fully. 
            // But `GetOrderByCorrelationId` is for Return Endpoint, which might just need status and amount?
            // The user didn't specify what Return Endpoint effectively does besides "redirect consumer".
        };
    }
}
