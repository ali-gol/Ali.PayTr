using Microsoft.EntityFrameworkCore;
using PayTr.Payment.EFCore.Extensions;

namespace PayTr.Payment.Sample;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyPayTrPaymentModels();
    }
}
