using Ali.PayTr.Abstractions.Interfaces;
using Ali.PayTr.Abstractions.Models;
using Ali.PayTr.Abstractions.Options;
using Ali.PayTr.Core.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Globalization;
using System.Linq;

namespace Ali.PayTr.Core.Clients;

public sealed class PayTrClient : IPayTrClient
{
    private readonly HttpClient _httpClient;
    private readonly PayTrOptions _options;
    private readonly IPayTrHashService _hashService;
    private readonly ILogger<PayTrClient> _logger;

    public PayTrClient(
            HttpClient httpClient,
            IOptions<PayTrOptions> options,
            IPayTrHashService hashService,
            ILogger<PayTrClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _hashService = hashService;
        _logger = logger;
    }

    public string ConvertAmountToString(decimal amount)
    {
        return ((long)Math.Round(amount * 100, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture);
    }

    private string BuildReturnUrl(string pattern, Guid correlationId)
    {
        var path = pattern.Replace("{correlationId}", correlationId.ToString())
                          .TrimStart('/');
        return $"{_options.SiteUrl.TrimEnd('/')}/{path}";
    }

    public async Task<PayTrCreatePaymentResponse> CreatePaymentAsync(PayTrCreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.CorrelationId == Guid.Empty)
        {
            return new PayTrCreatePaymentResponse { IsSuccess = false, CorrelationId = request.CorrelationId, Message = "CorrelationId must be set." };
        }
        if (request.PaymentAmount <= 0)
        {
            return new PayTrCreatePaymentResponse { IsSuccess = false, CorrelationId = request.CorrelationId, Message = "PaymentAmount must be greater than zero." };
        }
        if (request.BasketItems == null || !request.BasketItems.Any())
        {
            return new PayTrCreatePaymentResponse { IsSuccess = false, CorrelationId = request.CorrelationId, Message = "BasketItems cannot be empty." };
        }
        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
        {
            return new PayTrCreatePaymentResponse { IsSuccess = false, CorrelationId = request.CorrelationId, Message = "CustomerEmail cannot be empty." };
        }

        var paytrToken = _hashService.CreateIFrameToken(request, _options);

        var userIp = request.ClientIp ?? "127.0.0.1";
        var merchantOid = MerchantOidConverter.ToMerchantOid(request.CorrelationId);
        var paymentAmountStr = ConvertAmountToString(request.PaymentAmount);

        var basketJson = JsonSerializer.Serialize(request.BasketItems.Select(x => new object[] { x.Name, x.Price.ToString("0.00", CultureInfo.InvariantCulture), x.Quantity }));
        var basketBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(basketJson));

        var noInstallment = request.InstallmentCount == 1 ? "1" : "0";
        var maxInstallment = request.InstallmentCount == 1 ? "0" : request.InstallmentCount.ToString();

        var postData = new Dictionary<string, string>
        {
            ["merchant_id"] = _options.MerchantId,
            ["user_ip"] = userIp,
            ["merchant_oid"] = merchantOid,
            ["email"] = request.CustomerEmail,
            ["payment_amount"] = paymentAmountStr,
            ["paytr_token"] = paytrToken,
            ["user_basket"] = basketBase64,
            ["debug_on"] = _options.DebugMode ? "1" : "0",
            ["no_installment"] = noInstallment,
            ["max_installment"] = maxInstallment,
            ["user_name"] = request.CustomerFullName ?? string.Empty,
            ["user_address"] = request.CustomerAddress ?? string.Empty,
            ["user_phone"] = request.CustomerPhone ?? string.Empty,
            ["merchant_ok_url"] = BuildReturnUrl(_options.SuccessUrlPattern, request.CorrelationId),
            ["merchant_fail_url"] = BuildReturnUrl(_options.FailUrlPattern, request.CorrelationId),
            ["timeout_limit"] = (request.TimeoutLimitMinutes ?? _options.TimeoutLimitMinutes).ToString(),
            ["currency"] = request.Currency ?? "TL",
            ["test_mode"] = _options.TestMode ? "1" : "0",
            ["lang"] = request.Language ?? _options.Language ?? "tr"
        };

        try
        {
            using var response = await _httpClient.PostAsync("odeme/api/get-token", new FormUrlEncodedContent(postData), cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            
            _logger.LogTrace("PayTR api response received: {CorrelationId}\n{ResponseString}", request.CorrelationId, responseString);
            
            using var doc = JsonDocument.Parse(responseString);
            var status = doc.RootElement.GetProperty("status").GetString();
            if (status == "success")
            {
                var token = doc.RootElement.GetProperty("token").GetString();
                return new PayTrCreatePaymentResponse
                {
                    IsSuccess = true,
                    CorrelationId = request.CorrelationId,
                    RedirectUrl = $"https://www.paytr.com/odeme/guvenli/{token}",
                    Token = token
                };
            }
            else
            {
                var reason = "Unknown";
                if (doc.RootElement.TryGetProperty("reason", out var r))
                    reason = r.GetString();
                
                _logger.LogError("PayTR Api unknown error. CorrelationId:{CorrelationId}: reason:{Reason}", request.CorrelationId, reason);
                return new PayTrCreatePaymentResponse
                {
                    IsSuccess = false,
                    CorrelationId = request.CorrelationId,
                    Message = reason
                };
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid response format from PayTR for CorrelationId: {CorrelationId}", request.CorrelationId);
            return new PayTrCreatePaymentResponse
            {
                IsSuccess = false,
                CorrelationId = request.CorrelationId,
                Message = "Invalid response format from PayTR."
            };
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
        {
            _logger.LogError(ex, "Network error occurred while calling PayTR API for CorrelationId: {CorrelationId}", request.CorrelationId);
            return new PayTrCreatePaymentResponse
            {
                IsSuccess = false,
                CorrelationId = request.CorrelationId,
                Message = "Network error occurred while communicating with PayTR. Please try again."
            };
        }
    }
}
