using Ali.PayTr.Abstractions.Interfaces;
using Ali.PayTr.Abstractions.Models;
using Ali.PayTr.Abstractions.Options;
using Ali.PayTr.Core.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Ali.PayTr.Core.Services;

public sealed class PayTrQueryService : IPayTrQueryService
{
    private readonly HttpClient _httpClient;
    private readonly PayTrOptions _options;
    private readonly IPayTrHashService _hashService;
    private readonly ILogger<PayTrQueryService> _logger;

    public PayTrQueryService(
        HttpClient httpClient,
        IOptions<PayTrOptions> options,
        IPayTrHashService hashService,
        ILogger<PayTrQueryService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _hashService = hashService;
        _logger = logger;
    }

    public async Task<PayTrQueryResponse> QueryStatusAsync(PayTrQueryRequest request, CancellationToken cancellationToken = default)
    {
        var merchantOid = MerchantOidConverter.ToMerchantOid(request.CorrelationId);

        var hashStr = _options.MerchantId + merchantOid + _options.MerchantSalt;
        var paytrToken = _hashService.CreateTokenHash(hashStr, _options.MerchantKey, "");

        var postData = new Dictionary<string, string>
        {
            ["merchant_id"] = _options.MerchantId,
            ["merchant_oid"] = merchantOid,
            ["paytr_token"] = paytrToken
        };

        using var response = await _httpClient.PostAsync("odeme/durum-sorgu", new FormUrlEncodedContent(postData), cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogTrace($"PayTR status query api response received: {request.CorrelationId}\n{responseString}");

        try
        {
            using var doc = JsonDocument.Parse(responseString);
            var status = doc.RootElement.GetProperty("status").GetString();

            if (status == "success")
            {
                var paymentStatus = doc.RootElement.TryGetProperty("payment_status", out var pStatus) ? pStatus.GetString() : null;
                var paymentAmount = doc.RootElement.TryGetProperty("payment_amount", out var pAmount) && decimal.TryParse(pAmount.GetString(), out var amount) ? amount / 100M : (decimal?)null;
                
                return new PayTrQueryResponse
                {
                    IsSuccess = true,
                    PaymentStatus = paymentStatus,
                    PaymentAmount = paymentAmount,
                    FailedReasonCode = doc.RootElement.TryGetProperty("failed_reason_code", out var frc) ? frc.GetString() : null,
                    FailedReasonMessage = doc.RootElement.TryGetProperty("failed_reason_msg", out var frm) ? frm.GetString() : null
                };
            }
            else
            {
                var reason = doc.RootElement.TryGetProperty("err_msg", out var err) ? err.GetString() : "Unknown Error";
                _logger.LogError($"PayTR Query failed. CorrelationId:{request.CorrelationId}, reason:{reason}");
                return new PayTrQueryResponse
                {
                    IsSuccess = false,
                    Message = reason
                };
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, $"Failed to parse PayTR query response for {request.CorrelationId}.");
            return new PayTrQueryResponse
            {
                IsSuccess = false,
                Message = "Invalid response from PayTR."
            };
        }
    }
}
