namespace Ali.PayTr.Abstractions.Models;

public sealed class PayTrCreatePaymentResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public string? RedirectUrl { get; set; }
    public Guid CorrelationId { get; set; }
}
