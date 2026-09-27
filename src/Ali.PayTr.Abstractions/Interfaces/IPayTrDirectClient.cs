using System.Threading;
using System.Threading.Tasks;
using Ali.PayTr.Abstractions.Models;

namespace Ali.PayTr.Abstractions.Interfaces;

public interface IPayTrDirectClient
{
    /// <summary>
    /// Executes a Direct API payment request (POST to https://www.paytr.com/odeme).
    /// </summary>
    Task<PayTrDirectPaymentResult> CreatePaymentAsync(PayTrDirectPaymentRequest request, CancellationToken cancellationToken = default);
}
