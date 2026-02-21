namespace PayTr.Payment.Abstractions.Models;

public sealed class PayTrBasketItem
{
    public string Name { get; set; } = default!;
    public string Price { get; set; } = default!;
    public int Quantity { get; set; }
}