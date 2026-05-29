namespace Ali.PayTr.Abstractions.Models;

public sealed class PayTrPaymentSuccessNotification
{
    public string MerchantId { get; set; } = default!;
    public string MerchantOid { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string TotalAmount { get; set; } = default!;
    public string Hash { get; set; } = default!;
    public string PaymentType { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public string TestMode { get; set; } = default!;
    public string RawNotificationBody { get; set; }
}
