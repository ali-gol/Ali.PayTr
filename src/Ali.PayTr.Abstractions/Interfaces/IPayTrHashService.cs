using Ali.PayTr.Abstractions.Models;
using Ali.PayTr.Abstractions.Options;

namespace Ali.PayTr.Abstractions.Interfaces;

public interface IPayTrHashService
{
    /// <summary>
    /// Computes HMAC-SHA256 based on PayTR standard.
    /// </summary>
    string CreateTokenHash(string hashStr, string merchantKey);

    /// <summary>
    /// Constructs the exact string required for the IFrame API token and hashes it.
    /// </summary>
    string CreateIFrameToken(PayTrCreatePaymentRequest request, PayTrOptions options);

    /// <summary>
    /// Constructs the exact string required for the Direct API token and hashes it.
    /// </summary>
    string CreateDirectApiToken(PayTrDirectPaymentRequest request, PayTrOptions options);
}
