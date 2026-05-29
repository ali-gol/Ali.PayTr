namespace Ali.PayTr.Abstractions.Models;

public sealed class PayTrRefundResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? RefundAmount { get; set; }
    public string? ReferenceNo { get; set; }
}
