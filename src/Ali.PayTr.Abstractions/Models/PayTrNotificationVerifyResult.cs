namespace Ali.PayTr.Abstractions.Models;
public record PayTrNotificationVerifyResult
{
    public bool IsVerificationSuccessful { get; set; }
    public string ExpectedHash { get; set; }
    public string ReceivedHash { get; set; }
    public string? FailureReason { get; set; }
}
