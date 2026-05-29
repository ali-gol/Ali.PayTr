using Ali.PayTr.Abstractions.Models;

namespace Ali.PayTr.Abstractions.Interfaces;

public interface IPayTrOrderService
{
    Task<PayTrCreatePaymentResponse> CreateOrderAndGetPaymentUrlAsync(PayTrCreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task<PayTrOrderModel?> GetOrderByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default);
}