namespace PayTr.Payment.Abstractions.Options;

public sealed class PayTrOptions
{
    public string MerchantId { get; set; } = default!;
    public string MerchantKey { get; set; } = default!;
    public string MerchantSalt { get; set; } = default!;

    /// <summary>
    /// PAYTR token request içinde gönderilen user_ip alanı.
    /// Eğer doğru yönetmek istersen endpoint içinde gerçek client ip alıp set edebilirsin.
    /// </summary>
    public string ServerIp { get; set; } = "";

    public string Currency { get; set; } = "TRY";
    public bool TestMode { get; set; } = true;
    public string Language { get; set; } = "tr";
    public string SiteUrl { get; set; }
    /// <summary>
    /// Default: https://www.paytr.com/
    /// </summary>
    public string BaseUrl { get; set; } = "https://www.paytr.com/";
}

