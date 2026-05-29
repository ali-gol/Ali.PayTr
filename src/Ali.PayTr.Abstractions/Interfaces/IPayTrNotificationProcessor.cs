using Ali.PayTr.Abstractions.Models;

namespace Ali.PayTr.Abstractions.Interfaces;

public interface IPayTrNotificationProcessor
{
    Task<PayTrNotificationVerifyResult> ProcessNotificationAsync(PayTrNotificationRequest notification, CancellationToken cancellationToken = default);
}
