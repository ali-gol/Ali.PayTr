using Ali.PayTr.Abstractions.Models;

namespace Ali.PayTr.Abstractions.Interfaces;

public interface IPayTrClient
{
    Task<PayTrCreatePaymentResponse> CreatePaymentAsync(PayTrCreatePaymentRequest request, CancellationToken cancellationToken = default);
}
