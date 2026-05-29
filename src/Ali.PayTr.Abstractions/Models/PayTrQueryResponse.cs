namespace Ali.PayTr.Abstractions.Models;

public sealed class PayTrQueryResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }

    /// <summary>
    /// The payment status returned by PayTR.
    /// Can be 'Success', 'Failed', etc.
    /// </summary>
    public string? PaymentStatus { get; set; }

    /// <summary>
    /// Original payment amount.
    /// </summary>
    public decimal? PaymentAmount { get; set; }

    /// <summary>
    /// The error code if the payment failed.
    /// </summary>
    public string? FailedReasonCode { get; set; }

    /// <summary>
    /// The error message if the payment failed.
    /// </summary>
    public string? FailedReasonMessage { get; set; }
}
