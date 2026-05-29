using Ali.PayTr.Core.Entities;

namespace Ali.PayTr.Core.Interfaces;

public interface IPayTrRepository
{
    Task<PayTrOrder?> GetOrderById(Guid orderId, CancellationToken cancellationToken = default);
    Task<PayTrOrder?> GetOrderByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default);

    Task AddOrderAsync(PayTrOrder order, CancellationToken cancellationToken = default);
    Task UpdateOrderAsync(PayTrOrder order, CancellationToken cancellationToken = default);
    Task AddLogAsync(PayTrOrderLogHistory log, CancellationToken cancellationToken = default);
    Task AddNotificationAsync(PayTrNotificationHistory notification, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}