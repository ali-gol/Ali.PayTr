using Ali.PayTr.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ali.PayTr.EFCore.Configurations;

public class PayTrOrderConfiguration : IEntityTypeConfiguration<PayTrOrder>
{
    private readonly PayTrEFCoreOptions _options;

    public PayTrOrderConfiguration(PayTrEFCoreOptions options)
    {
        _options = options;
    }

    public void Configure(EntityTypeBuilder<PayTrOrder> builder)
    {
        builder.ToTable($"{_options.TablePrefix}Orders", _options.Schema);

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.CorrelationId)
            .IsUnique();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.UserId);
        

        builder.HasMany(x => x.LogHistory)
            .WithOne(x => x.PayTrOrder)
            .HasForeignKey(x => x.PayTrOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Notifications)
            .WithOne(x => x.PayTrOrder)
            .HasForeignKey(x => x.PayTrOrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
