using Ali.PayTr.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Ali.PayTr.AspNetCore.Services;

internal sealed class PayTrClientIpAccessor : IPayTrClientIpAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PayTrClientIpAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetClientIp()
    {
        return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    }
}
