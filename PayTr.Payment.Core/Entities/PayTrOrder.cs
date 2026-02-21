using PayTr.Payment.Abstractions.Enums;

namespace PayTr.Payment.Core.Entities;

public class PayTrOrder
{
    public Guid Id { get; set; }
    
    public Guid? UserId { get; set; }
        
    public string Status { get; set; } = "Pending"; // Initial status
    public decimal TotalAmount { get; set; }
    
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
    public string? BasketJson { get; set; }
    public string? Currency { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerFullName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? ClientIp { get; set; }
    public Guid CorrelationId { get; set; }
    public PaymentType PaymentType { get; set; }
    public ICollection<PayTrOrderLogHistory> LogHistory { get; set; } = new List<PayTrOrderLogHistory>();
    public ICollection<PayTrNotificationHistory> Notifications { get; set; } = new List<PayTrNotificationHistory>();
    public int InstallmentCount { get; set; }
}
