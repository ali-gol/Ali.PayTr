using Ali.PayTr.Abstractions.Models;

namespace Ali.PayTr.Abstractions.Interfaces;

public interface IPayTrQueryService
{
    /// <summary>
    /// Queries the status of an existing payment directly from PayTR.
    /// Useful if webhooks fail to deliver.
    /// </summary>
    Task<PayTrQueryResponse> QueryStatusAsync(PayTrQueryRequest request, CancellationToken cancellationToken = default);
}
