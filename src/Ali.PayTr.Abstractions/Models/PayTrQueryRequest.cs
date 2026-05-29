namespace Ali.PayTr.Abstractions.Models;

public sealed class PayTrQueryRequest
{
    /// <summary>
    /// The unique correlation id (merchant_oid) of the order to query.
    /// </summary>
    public Guid CorrelationId { get; set; }
}
