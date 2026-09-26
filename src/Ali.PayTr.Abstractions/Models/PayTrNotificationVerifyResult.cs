namespace Ali.PayTr.Abstractions.Models;
public record PayTrNotificationVerifyResult
{
    public bool IsVerificationSuccessful { get; set; }
    public required string ExpectedHash { get; set; }
    public required string ReceivedHash { get; set; }
    public string? FailureReason { get; set; }
}
