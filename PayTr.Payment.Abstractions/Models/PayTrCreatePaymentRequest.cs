using PayTr.Payment.Abstractions.Models;

namespace PayTr.Payment.Abstractions.Models;

public sealed class PayTrCreatePaymentRequest
{
    public Guid? UserId { get; set; }
    public string ClientIp { get; set; } = default!;
    public string CustomerFullName { get; set; }
    public string CustomerAddress { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerPhone { get; set; }
    public Guid CorrelationId { get; set; } = default!;
    public decimal PaymentAmount { get; set; }
    public string PaymentType { get; set; } = "card";


    public string Currency { get; set; } = "TL";
    public int InstallmentCount { get; set; } = 0;
    
    public string? Language { get; set; }
    public string? OkUrl { get; set; }
    public string? FailUrl { get; set; }

    public List<PayTrBasketItem> BasketItems { get; set; } = new();
}
