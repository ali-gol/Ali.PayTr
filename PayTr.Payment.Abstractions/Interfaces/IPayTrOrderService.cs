using PayTr.Payment.Abstractions.Models;

namespace PayTr.Payment.Abstractions.Interfaces;

public interface IPayTrOrderService
{
    Task<PayTrCreatePaymentResponse> CreateOrderAsync(PayTrCreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task<PayTrOrderModel?> GetOrderByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default);
}