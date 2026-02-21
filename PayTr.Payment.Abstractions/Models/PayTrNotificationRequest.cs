namespace PayTr.Payment.Abstractions.Models;

public sealed class PayTrNotificationRequest
{
    // Keeping properties that mapping from form fields
    public string MerchantOid { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string TotalAmount { get; set; } = default!;
    public string Hash { get; set; } = default!;
    public int FailedReasonCode { get; set; } = default!;
    public string FailedReasonMsg { get; set; } = default!;
    public string FailedReasonDescription { get; set; }
    public string PaymentType { get; set; } = default!; // card, etc.
    public string Currency { get; set; } = default!;
    public string TestMode { get; set; } = default!;

    public IDictionary<string, string>? RawForm { get; set; }
}
