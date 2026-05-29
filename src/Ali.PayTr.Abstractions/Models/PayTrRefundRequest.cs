namespace Ali.PayTr.Abstractions.Models;

public sealed class PayTrRefundRequest
{
    /// <summary>
    /// The unique correlation id (merchant_oid) of the original order to refund.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// The amount to refund. E.g. for a 150.50 TRY payment, you can refund 150.50.
    /// </summary>
    public decimal ReturnAmount { get; set; }

    /// <summary>
    /// Optional reference number for the merchant's own tracking.
    /// </summary>
    public string? ReferenceNo { get; set; }
}
