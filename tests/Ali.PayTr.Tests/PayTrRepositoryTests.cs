using Ali.PayTr.Core.Entities;
using Ali.PayTr.EFCore.Extensions;
using Ali.PayTr.EFCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ali.PayTr.Tests;
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyPayTrPaymentModels();
        base.OnModelCreating(modelBuilder);
    }
}

public class PayTrRepositoryTests
{
    private DbContextOptions<TestDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task AddOrderAsync_ShouldAddOrderToDatabase()
    {
        // Arrange
        var options = CreateNewContextOptions();
        var correlationId = Guid.NewGuid();
        var order = new PayTrOrder { Id = Guid.NewGuid(), CorrelationId = correlationId, TotalAmount = 100m };

        // Act
        using (var context = new TestDbContext(options))
        {
            var repository = new PayTrRepository<TestDbContext>(context);
            await repository.AddOrderAsync(order);
            await repository.SaveChangesAsync();
        }

        // Assert
        using (var context = new TestDbContext(options))
        {
            var savedOrder = await context.Set<PayTrOrder>().FirstOrDefaultAsync(o => o.CorrelationId == correlationId);
            Assert.NotNull(savedOrder);
            Assert.Equal(100m, savedOrder.TotalAmount);
        }
    }

    [Fact]
    public async Task UpdateOrderAsync_ShouldUpdateOrderInDatabase()
    {
        // Arrange
        var options = CreateNewContextOptions();
        var correlationId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = new PayTrOrder { Id = orderId, CorrelationId = correlationId, TotalAmount = 100m, Status = "Created" };

        using (var context = new TestDbContext(options))
        {
            context.Set<PayTrOrder>().Add(order);
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new TestDbContext(options))
        {
            var repository = new PayTrRepository<TestDbContext>(context);
            var orderToUpdate = await repository.GetOrderByCorrelationIdAsync(correlationId);
            orderToUpdate!.Status = "Completed";
            await repository.UpdateOrderAsync(orderToUpdate);
            await repository.SaveChangesAsync();
        }

        // Assert
        using (var context = new TestDbContext(options))
        {
            var updatedOrder = await context.Set<PayTrOrder>().FirstOrDefaultAsync(o => o.CorrelationId == correlationId);
            Assert.NotNull(updatedOrder);
            Assert.Equal("Completed", updatedOrder.Status);
        }
    }
}
