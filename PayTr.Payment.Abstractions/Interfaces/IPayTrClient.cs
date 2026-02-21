using PayTr.Payment.Abstractions.Models;

namespace PayTr.Payment.Abstractions.Interfaces;

public interface IPayTrClient
{
    Task<PayTrCreatePaymentResponse> CreatePaymentAsync(PayTrCreatePaymentRequest request, CancellationToken cancellationToken = default);
}
