namespace PayTr.Payment.Abstractions.Models;

public sealed class OrderPayTrNotificationDto
{
    public string merchant_id { get; set; } = default!;
    public string merchant_oid { get; set; } = default!;
    public string status { get; set; } = default!;
    public string total_amount { get; set; } = default!;
    public string hash { get; set; } = default!;
    public string failed_reason_code { get; set; } = default!;
    public string failed_reason_msg { get; set; } = default!;
    public string payment_type { get; set; } = default!;
    public string currency { get; set; } = default!;
    public string test_mode { get; set; } = default!;
}
