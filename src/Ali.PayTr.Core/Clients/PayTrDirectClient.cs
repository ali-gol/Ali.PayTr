using Ali.PayTr.Abstractions.Interfaces;
using Ali.PayTr.Abstractions.Models;
using Ali.PayTr.Abstractions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Ali.PayTr.Core.Clients;

public sealed class PayTrDirectClient : IPayTrDirectClient
{
    private readonly HttpClient _httpClient;
    private readonly PayTrOptions _options;
    private readonly IPayTrHashService _hashService;
    private readonly ILogger<PayTrDirectClient> _logger;

    public PayTrDirectClient(
            HttpClient httpClient,
            IOptions<PayTrOptions> options,
            IPayTrHashService hashService,
            ILogger<PayTrDirectClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _hashService = hashService;
        _logger = logger;
    }

    private string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 6) return "****";
        var first6 = cardNumber.Substring(0, 6);
        var last4 = cardNumber.Length >= 10 ? cardNumber.Substring(cardNumber.Length - 4) : "";
        return $"{first6}{new string('*', cardNumber.Length - 10)}{last4}";
    }

    public async Task<PayTrDirectPaymentResult> CreatePaymentAsync(PayTrDirectPaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new PayTrDirectPaymentResult { IsSuccess = false, Message = "Email cannot be empty." };
        }
        if (request.PaymentAmount <= 0)
        {
            return new PayTrDirectPaymentResult { IsSuccess = false, Message = "PaymentAmount must be greater than zero." };
        }

        var paytrToken = _hashService.CreateDirectApiToken(request, _options);

        var paymentAmountStr = request.PaymentAmount.ToString("0.00", CultureInfo.InvariantCulture);

        var postData = new Dictionary<string, string>
        {
            ["merchant_id"] = _options.MerchantId,
            ["user_ip"] = request.UserIp ?? "127.0.0.1",
            ["merchant_oid"] = request.MerchantOid,
            ["email"] = request.Email,
            ["payment_amount"] = paymentAmountStr,
            ["payment_type"] = request.PaymentType,
            ["installment_count"] = request.InstallmentCount.ToString(CultureInfo.InvariantCulture),
            ["paytr_token"] = paytrToken,
            ["cc_owner"] = request.CcOwner,
            ["card_number"] = request.CardNumber,
            ["expiry_month"] = request.ExpiryMonth,
            ["expiry_year"] = request.ExpiryYear,
            ["cvv"] = request.Cvv,
            ["merchant_ok_url"] = request.MerchantOkUrl,
            ["merchant_fail_url"] = request.MerchantFailUrl,
            ["user_name"] = request.UserName,
            ["user_address"] = request.UserAddress,
            ["user_phone"] = request.UserPhone,
            ["client_lang"] = request.ClientLang,
            ["sync_mode"] = request.SyncMode ? "1" : "0",
            ["non_3d"] = request.Non3D ? "1" : "0",
            ["test_mode"] = (request.TestMode ?? _options.TestMode) ? "1" : "0",
            ["debug_on"] = request.DebugOn || _options.DebugMode ? "1" : "0"
        };

        if (!string.IsNullOrWhiteSpace(request.CardType))
            postData["card_type"] = request.CardType;

        if (request.Non3DTestFailed)
            postData["non3d_test_failed"] = "1";

        if (request.RequestExpDate.HasValue)
            postData["request_exp_date"] = request.RequestExpDate.Value.ToString(CultureInfo.InvariantCulture);

        if (request.Currency != "TL")
            postData["currency"] = request.Currency;

        if (request.UserBasket?.Any() == true)
        {
            var basketJson = JsonSerializer.Serialize(request.UserBasket.Select(x => new object[] { x.Name, x.Price.ToString("0.00", CultureInfo.InvariantCulture), x.Quantity }));
            postData["user_basket"] = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(basketJson));
        }

        try
        {
            _logger.LogInformation("Sending Direct API payment request for OID {MerchantOid}. Card: {CardMasked}", request.MerchantOid, MaskCardNumber(request.CardNumber));
            
            using var response = await _httpClient.PostAsync("odeme", new FormUrlEncodedContent(postData), cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Mask any potential sensitive response log, though PayTR typically doesn't echo PCI data
            _logger.LogTrace("PayTR Direct API response received for OID {MerchantOid}:\n{ResponseString}", request.MerchantOid, responseString);
            
            var resultObject = JsonSerializer.Deserialize<PayTrDirectPaymentResponse>(responseString);
            
            if (resultObject == null)
            {
                return new PayTrDirectPaymentResult { IsSuccess = false, Message = "Failed to parse JSON response." };
            }

            return new PayTrDirectPaymentResult
            {
                IsSuccess = resultObject.IsSuccess || resultObject.IsWaitCallback,
                Message = resultObject.Msg ?? resultObject.Reason,
                Response = resultObject
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid response format from PayTR Direct API for OID: {MerchantOid}", request.MerchantOid);
            return new PayTrDirectPaymentResult
            {
                IsSuccess = false,
                Message = "Invalid response format from PayTR."
            };
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
        {
            _logger.LogError(ex, "Network error occurred while calling PayTR Direct API for OID: {MerchantOid}", request.MerchantOid);
            return new PayTrDirectPaymentResult
            {
                IsSuccess = false,
                Message = "Network error occurred while communicating with PayTR. Please try again."
            };
        }
    }
}
