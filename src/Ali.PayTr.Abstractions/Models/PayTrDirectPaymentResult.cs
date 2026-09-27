using System;

namespace Ali.PayTr.Abstractions.Models;

public sealed class PayTrDirectPaymentResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public Guid CorrelationId { get; set; }
    
    // Direct API Specific
    public PayTrDirectPaymentResponse? Response { get; set; }
}
