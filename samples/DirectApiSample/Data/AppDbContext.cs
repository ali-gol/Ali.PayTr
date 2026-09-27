using Ali.PayTr.EFCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DirectApiSample.Data;

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
