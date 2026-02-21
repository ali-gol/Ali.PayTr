using PayTr.Payment.Abstractions.Enums;

namespace PayTr.Payment.Abstractions.Models;

public sealed class PayTrOrderModel
{
    public Guid CorrelationId { get; set; }
    public decimal PaymentAmount { get; set; }
    public int InstallmentCount { get; set; }
    public List<PayTrBasketItem> BasketItems { get; set; } = new();

    public string CustomerFullName { get; set; } = default!;
    public string CustomerAddress { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;
    public string CustomerEmail { get; set; } = default!;
    public PaymentType PaymentType { get; set; }
}
