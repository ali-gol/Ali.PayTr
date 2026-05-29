using System.ComponentModel.DataAnnotations;

namespace Ali.PayTr.Abstractions.Options;

public sealed record PayTrOptions
{
    [Required(AllowEmptyStrings = false)] public string MerchantId { get; set; } = default!;

    [Required(AllowEmptyStrings = false)] public string MerchantKey { get; set; } = default!;

    [Required(AllowEmptyStrings = false)] public string MerchantSalt { get; set; } = default!;

    public string ServerIp { get; set; } = "";

    [Required(AllowEmptyStrings = false)] public string Currency { get; set; } = "TRY";

    [Required(AllowEmptyStrings = false)] public bool TestMode { get; set; }

    [Required] public string Language { get; set; } = "tr";

    [Required] public string SiteUrl { get; set; } = default!;
    [Required] public string RoutePrefix { get; set; } = "paytr";
    [Required] public string SuccessUrlPattern { get; set; } = "success/{correlationId}";
    [Required] public string FailUrlPattern { get; set; } = "fail/{correlationId}";

    [Required, Url] public string ApiBaseUrl { get; set; } = "https://www.paytr.com/";
}
