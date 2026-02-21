namespace PayTr.Payment.Core.Entities;

public class PayTrOrderLogHistory
{
    public Guid Id { get; set; }
    
    public Guid PayTrOrderId { get; set; }
    public PayTrOrder PayTrOrder { get; set; } = default!;
    
    public string Message { get; set; } = default!;
    public string? OldStatus { get; set; }
    public string? NewStatus { get; set; }
    
    public DateTimeOffset CreatedDate { get; set; }
    public Guid CorrelationId { get; internal set; }
}
