using Ali.PayTr.Abstractions.Interfaces;
using Ali.PayTr.Abstractions.Models;
using Ali.PayTr.Abstractions.Options;
using Ali.PayTr.Core.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Ali.PayTr.Core.Services;

public sealed class PayTrRefundService : IPayTrRefundService
{
    private readonly HttpClient _httpClient;
    private readonly PayTrOptions _options;
    private readonly IPayTrHashService _hashService;
    private readonly ILogger<PayTrRefundService> _logger;

    public PayTrRefundService(
        HttpClient httpClient,
        IOptions<PayTrOptions> options,
        IPayTrHashService hashService,
        ILogger<PayTrRefundService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _hashService = hashService;
        _logger = logger;
    }

    public async Task<PayTrRefundResponse> RefundAsync(PayTrRefundRequest request, CancellationToken cancellationToken = default)
    {
        var merchantOid = MerchantOidConverter.ToMerchantOid(request.CorrelationId);
        var returnAmountStr = request.ReturnAmount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

        var hashStr = _options.MerchantId + merchantOid + returnAmountStr + _options.MerchantSalt;
        var paytrToken = _hashService.CreateTokenHash(hashStr, _options.MerchantKey, "");

        var postData = new Dictionary<string, string>
        {
            ["merchant_id"] = _options.MerchantId,
            ["merchant_oid"] = merchantOid,
            ["return_amount"] = returnAmountStr,
            ["paytr_token"] = paytrToken
        };

        if (!string.IsNullOrWhiteSpace(request.ReferenceNo))
        {
            postData["reference_no"] = request.ReferenceNo;
        }

        using var response = await _httpClient.PostAsync("odeme/iade", new FormUrlEncodedContent(postData), cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogTrace($"PayTR refund api response received: {request.CorrelationId}\n{responseString}");
        
        try
        {
            using var doc = JsonDocument.Parse(responseString);
            var status = doc.RootElement.GetProperty("status").GetString();

            if (status == "success")
            {
                var isReturnAmountPresent = doc.RootElement.TryGetProperty("return_amount", out var amountProp);
                return new PayTrRefundResponse
                {
                    IsSuccess = true,
                    RefundAmount = isReturnAmountPresent ? amountProp.GetString() : returnAmountStr
                };
            }
            else
            {
                var reason = doc.RootElement.TryGetProperty("err_msg", out var err) ? err.GetString() : "Unknown Error";
                _logger.LogError($"PayTR Refund failed. CorrelationId:{request.CorrelationId}, reason:{reason}");
                return new PayTrRefundResponse
                {
                    IsSuccess = false,
                    Message = reason
                };
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, $"Failed to parse PayTR refund response for {request.CorrelationId}.");
            return new PayTrRefundResponse
            {
                IsSuccess = false,
                Message = "Invalid response from PayTR."
            };
        }
    }
}
