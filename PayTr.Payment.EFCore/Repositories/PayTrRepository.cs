using Microsoft.EntityFrameworkCore;
using PayTr.Payment.Core.Entities;
using PayTr.Payment.Core.Interfaces;
using System.Threading;

namespace PayTr.Payment.EFCore.Repositories;

// Need to be generic or accept DbContext?
// Requirements: "Consumer owns database provider choice... Package should support any EF Core provider".
// "builder.Services.AddPayTrPaymentsEFCore<AppDbContext>();"
// So the Repository should probably depend on `TContext` where `TContext : DbContext`.
// But `IPayTrRepository` is not generic.
// So `PayTrRepository<TContext>` implements `IPayTrRepository`.

public class PayTrRepository<TContext> : IPayTrRepository where TContext : DbContext
{
    private readonly TContext _context;

    public PayTrRepository(TContext context)
    {
        _context = context;
    }

    public async Task AddOrderAsync(PayTrOrder order, CancellationToken cancellationToken = default)
    {
        await _context.Set<PayTrOrder>().AddAsync(order, cancellationToken);
    }

    public Task UpdateOrderAsync(PayTrOrder order, CancellationToken cancellationToken = default)
    {
        _context.Set<PayTrOrder>().Update(order);
        return Task.CompletedTask;
    }

    public async Task<PayTrOrder?> GetOrderById(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PayTrOrder>()
            .FirstOrDefaultAsync(x => x.CorrelationId == orderId, cancellationToken);
    }

    public async Task<PayTrOrder?> GetOrderByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PayTrOrder>()
            .FirstOrDefaultAsync(x => x.CorrelationId == correlationId, cancellationToken);
    }

    public async Task AddLogAsync(PayTrOrderLogHistory log, CancellationToken cancellationToken = default)
    {
        await _context.Set<PayTrOrderLogHistory>().AddAsync(log, cancellationToken);
    }

    public async Task AddNotificationAsync(PayTrNotificationHistory notification, CancellationToken cancellationToken = default)
    {
        await _context.Set<PayTrNotificationHistory>().AddAsync(notification, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
