using Ali.PayTr.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ali.PayTr.EFCore.Configurations;

public class PayTrOrderLogHistoryConfiguration : IEntityTypeConfiguration<PayTrOrderLogHistory>
{
    private readonly PayTrEFCoreOptions _options;

    public PayTrOrderLogHistoryConfiguration(PayTrEFCoreOptions options)
    {
        _options = options;
    }

    public void Configure(EntityTypeBuilder<PayTrOrderLogHistory> builder)
    {
        builder.ToTable($"{_options.TablePrefix}OrderLogHistories", _options.Schema);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Message)
            .IsRequired();
            
        builder.Property(x => x.OldStatus)
             .HasMaxLength(32);
             
        builder.Property(x => x.NewStatus)
             .HasMaxLength(32);
    }
}
