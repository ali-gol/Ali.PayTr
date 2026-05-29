using Ali.PayTr.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ali.PayTr.EFCore.Configurations;

public class PayTrNotificationHistoryConfiguration : IEntityTypeConfiguration<PayTrNotificationHistory>
{
    private readonly PayTrEFCoreOptions _options;

    public PayTrNotificationHistoryConfiguration(PayTrEFCoreOptions options)
    {
        _options = options;
    }

    public void Configure(EntityTypeBuilder<PayTrNotificationHistory> builder)
    {
        builder.ToTable($"{_options.TablePrefix}NotificationHistories", _options.Schema);

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.MerchantOid)
            .IsRequired()
            .HasMaxLength(64); // Match Order
            
        builder.HasIndex(x => x.MerchantOid);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.Hash)
            .HasMaxLength(128); // Standard SHA256 base64 is 44 chars, but allow buffer
            
        builder.Property(x => x.FailedReason)
            .HasMaxLength(512);

        builder.Property(x => x.RawBody)
             .HasColumnType("nvarchar(max)");
    }
}
