namespace PayTr.Payment.Core.Results;

public sealed class PayTrTokenResult
{
    public bool IsSuccess { get; set; }
    public string? Token { get; set; }
    public string? Reason { get; set; }
    public string RawResponse { get; set; } = default!;
}
