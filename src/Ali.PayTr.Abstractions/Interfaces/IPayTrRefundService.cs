using Ali.PayTr.Abstractions.Models;

namespace Ali.PayTr.Abstractions.Interfaces;

public interface IPayTrRefundService
{
    /// <summary>
    /// Initiates a refund for a previously successful payment.
    /// </summary>
    Task<PayTrRefundResponse> RefundAsync(PayTrRefundRequest request, CancellationToken cancellationToken = default);
}
