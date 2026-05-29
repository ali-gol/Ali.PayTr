namespace Ali.PayTr.Core.Entities;

public class PayTrNotificationHistory
{
    public Guid Id { get; set; }
    
    public Guid? PayTrOrderId { get; set; }
    public PayTrOrder? PayTrOrder { get; set; }
    
    public string MerchantOid { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string? RawBody { get; set; }
    public string? Hash { get; set; }
    public string? FailedReason { get; set; }
    
    public DateTimeOffset CreatedDate { get; set; }
}
