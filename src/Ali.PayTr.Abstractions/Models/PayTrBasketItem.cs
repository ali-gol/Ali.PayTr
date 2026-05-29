namespace Ali.PayTr.Abstractions.Models;

public sealed class PayTrBasketItem
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; } = default!;
    public int Quantity { get; set; }
}