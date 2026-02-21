using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayTr.Payment.Core.Entities;

namespace PayTr.Payment.EFCore.Configurations;

public class PayTrOrderLogHistoryConfiguration : IEntityTypeConfiguration<PayTrOrderLogHistory>
{
    public void Configure(EntityTypeBuilder<PayTrOrderLogHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Message)
            .IsRequired();
            
        builder.Property(x => x.OldStatus)
             .HasMaxLength(32);
             
        builder.Property(x => x.NewStatus)
             .HasMaxLength(32);
    }
}
