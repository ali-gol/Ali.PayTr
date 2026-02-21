using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayTr.Payment.Core.Entities;

namespace PayTr.Payment.EFCore.Configurations;

public class PayTrNotificationHistoryConfiguration : IEntityTypeConfiguration<PayTrNotificationHistory>
{
    public void Configure(EntityTypeBuilder<PayTrNotificationHistory> builder)
    {
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
             .HasColumnType("nvarchar(max)"); // or text
    }
}
