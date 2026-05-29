using Ali.PayTr.Abstractions.Enums;

namespace Ali.PayTr.Abstractions.Models;

public sealed class PayTrCreatePaymentRequest
{
    /// <summary>
    /// Gets or sets the IP address of the client associated with the current request.
    /// Will be used for monitoring and logging purposes, and may also be required by PayTR API for fraud prevention and security checks.
    /// </summary>
    public string ClientIp { get; set; } = default!;
    public string CustomerFullName { get; set; }
    public string CustomerAddress { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerPhone { get; set; }

    /// <summary>
    /// The CorrelationId is a unique identifier that can be used to correlate the payment request. It is recommended pass your own unique `PaymentId` on your system.
    /// It will be converted to MerchantOid in PayTR API, but we call it CorrelationId in our system for better clarity.
    /// This allows you to easily track and manage the payment process, and also helps PayTR support team to assist you better if you encounter any issues. 
    /// </summary>
    public Guid CorrelationId { get; set; } = default!;
    public decimal PaymentAmount { get; set; }
    public PaymentType PaymentType { get; set; }

    public string Currency { get; set; } = "TRY";
    public int InstallmentCount { get; set; } = 1;
    
    public string? Language { get; set; }
    public List<PayTrBasketItem> BasketItems { get; set; } = new();
}
