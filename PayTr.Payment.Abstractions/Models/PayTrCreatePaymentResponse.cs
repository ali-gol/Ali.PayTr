
namespace PayTr.Payment.Abstractions.Models;

public sealed class PayTrCreatePaymentResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public string? RedirectUrl { get; set; } // If PayTR returns a full URL or script
    public Guid CorrelationId { get; set; }
}
