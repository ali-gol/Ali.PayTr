namespace Ali.PayTr.Abstractions.Models;

public sealed class PayTrPaymentFailedNotification
{
    public string MerchantId { get; set; } = default!;
    public string MerchantOid { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string TotalAmount { get; set; } = default!;
    public string Hash { get; set; } = default!;
    public string PaymentType { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public string TestMode { get; set; } = default!;
    public string? FailedReasonCode { get; set; }
    public string FailedReasonMessage { get; set; } = default!;
    public string? FailedReasonDescription { get; set; }
    public string RawNotificationBody { get; set; }
    public bool IsSystemError { get; set; }
}
