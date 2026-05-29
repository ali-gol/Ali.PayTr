using Ali.PayTr.EFCore.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Ali.PayTr.EFCore.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplyPayTrPaymentModels(
            this ModelBuilder modelBuilder,
            Action<PayTrEFCoreOptions>? configureOptions = null)
    {
        // Default options'ı oluştur
        var options = new PayTrEFCoreOptions();

        // Kullanıcı bir ayar gönderdiyse onu uygula
        configureOptions?.Invoke(options);

        // Configuration sınıflarını options ile birlikte manuel register et
        modelBuilder.ApplyConfiguration(new PayTrOrderConfiguration(options));
        modelBuilder.ApplyConfiguration(new PayTrOrderLogHistoryConfiguration(options));
        modelBuilder.ApplyConfiguration(new PayTrNotificationHistoryConfiguration(options));

        return modelBuilder;
    }
}
