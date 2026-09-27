using System.Text.Json.Serialization;

namespace Ali.PayTr.Abstractions.Models;

public class PayTrDirectPaymentResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    // Reason/msg usually present if failed or as a generic description
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
    
    [JsonPropertyName("msg")]
    public string? Msg { get; set; }

    [JsonPropertyName("utoken")]
    public string? UToken { get; set; }

    [JsonPropertyName("ctoken")]
    public string? CToken { get; set; }

    public bool IsSuccess => Status == "success";
    public bool IsFailed => Status == "failed" || Status == "error";
    public bool IsWaitCallback => Status == "wait_callback";
}
