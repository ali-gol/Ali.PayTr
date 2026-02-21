using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayTr.Payment.Core.Entities;

namespace PayTr.Payment.EFCore.Configurations;

public class PayTrOrderConfiguration : IEntityTypeConfiguration<PayTrOrder>
{
    public void Configure(EntityTypeBuilder<PayTrOrder> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.CorrelationId)
            .IsUnique();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.UserId);
        
        // Relationships
        builder.HasMany(x => x.LogHistory)
            .WithOne(x => x.PayTrOrder)
            .HasForeignKey(x => x.PayTrOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Notifications)
            .WithOne(x => x.PayTrOrder)
            .HasForeignKey(x => x.PayTrOrderId)
            .OnDelete(DeleteBehavior.SetNull); // or Cascade? 
            // Notifications are historical records, maybe keep them if order deleted? 
            // But usually we don't delete Orders.
            // Requirement said "Order -> NotificationHistory is optional... PayTrOrderId should be nullable".
    }
}
