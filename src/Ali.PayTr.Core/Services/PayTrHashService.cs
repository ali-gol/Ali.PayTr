using Ali.PayTr.Abstractions.Interfaces;
using Ali.PayTr.Abstractions.Models;
using Ali.PayTr.Abstractions.Options;
using Ali.PayTr.Core.Utilities;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Ali.PayTr.Core.Services;

public sealed class PayTrHashService : IPayTrHashService
{
    public string CreateTokenHash(string hashStr, string merchantKey)
    {
        var key = Encoding.UTF8.GetBytes(merchantKey);
        var message = Encoding.UTF8.GetBytes(hashStr);
        
        using var hmac = new HMACSHA256(key);
        var hashValue = hmac.ComputeHash(message);
        return Convert.ToBase64String(hashValue);
    }

    public string CreateIFrameToken(PayTrCreatePaymentRequest request, PayTrOptions options)
    {
        var userIp = request.ClientIp ?? "127.0.0.1";
        var merchantOid = MerchantOidConverter.ToMerchantOid(request.CorrelationId);
        
        // IFrame expects amount in kuruş (amount * 100)
        var paymentAmountStr = ((long)Math.Round(request.PaymentAmount * 100, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture);

        var basketJson = JsonSerializer.Serialize(request.BasketItems?.Select(x => new object[] { x.Name, x.Price.ToString("0.00", CultureInfo.InvariantCulture), x.Quantity }) ?? Enumerable.Empty<object[]>());
        var basketBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(basketJson));

        var noInstallment = request.InstallmentCount == 1 ? "1" : "0";
        var maxInstallment = request.InstallmentCount == 1 ? "0" : request.InstallmentCount.ToString();

        // merchant_id + user_ip + merchant_oid + email + payment_amount + user_basket + no_installment + max_installment + currency + test_mode + merchant_salt
        var hashStr = string.Concat(
            options.MerchantId,
            userIp,
            merchantOid,
            request.CustomerEmail,
            paymentAmountStr,
            basketBase64,
            noInstallment,
            maxInstallment,
            request.Currency,
            options.TestMode ? "1" : "0",
            options.MerchantSalt
        );

        return CreateTokenHash(hashStr, options.MerchantKey);
    }

    public string CreateDirectApiToken(PayTrDirectPaymentRequest request, PayTrOptions options)
    {
        var userIp = request.UserIp ?? "127.0.0.1";
        
        // Direct API expects amount in standard decimal format (e.g. 100.99)
        var paymentAmountStr = request.PaymentAmount.ToString("0.00", CultureInfo.InvariantCulture);

        var testMode = request.TestMode ?? options.TestMode;
        var testModeStr = testMode ? "1" : "0";
        var non3DStr = request.Non3D ? "1" : "0";

        // merchant_id + user_ip + merchant_oid + email + payment_amount + payment_type + installment_count + currency + test_mode + non_3d + merchant_salt
        var hashStr = string.Concat(
            options.MerchantId,
            userIp,
            request.MerchantOid,
            request.Email,
            paymentAmountStr,
            request.PaymentType,
            request.InstallmentCount.ToString(CultureInfo.InvariantCulture),
            request.Currency,
            testModeStr,
            non3DStr,
            options.MerchantSalt
        );

        return CreateTokenHash(hashStr, options.MerchantKey);
    }
}