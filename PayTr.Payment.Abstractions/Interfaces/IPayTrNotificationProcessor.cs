using PayTr.Payment.Abstractions.Models;

namespace PayTr.Payment.Abstractions.Interfaces;

public interface IPayTrNotificationProcessor
{
    Task ProcessNotificationAsync(PayTrNotificationRequest notification, CancellationToken cancellationToken = default);
}
