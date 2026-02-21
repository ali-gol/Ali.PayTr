using Microsoft.EntityFrameworkCore;
using PayTr.Payment.EFCore.Configurations;

namespace PayTr.Payment.EFCore.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplyPayTrPaymentModels(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PayTrOrderConfiguration());
        modelBuilder.ApplyConfiguration(new PayTrOrderLogHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new PayTrNotificationHistoryConfiguration());
        
        return modelBuilder;
    }
}
