# PayTR Engineer Implementation Plan: IFrame API

## 1. Executive Implementation Summary
After reviewing the Business Analyst gap report and the Architect's design proposals, the PayTR IFrame API implementation is structurally sound and effectively complete for basic use cases. Previously identified critical issues (such as `JsonException` crashes, string-based hash validation timing vulnerabilities, and `debug_on` flag decoupling) are already resolved in the current codebase.

The remaining actionable work primarily revolves around the IP address detection mechanism.

## 2. Action Items

### 1. Implement `IPayTrClientIpAccessor`
- **What:** Create a service to automatically detect the client's IP address.
- **Why:** The `PayTrClient.cs` currently falls back to `127.0.0.1` if `ClientIp` is not provided in `PayTrCreatePaymentRequest`. We need to wire this up so that `IHttpContextAccessor` can automatically populate the IP in ASP.NET Core environments without polluting the `Ali.PayTr.Core` project with `Microsoft.AspNetCore.Http` dependencies.
- **How:** 
  1. Add `IPayTrClientIpAccessor` to `Ali.PayTr.Abstractions`.
  2. Implement `HttpContextClientIpAccessor` in `Ali.PayTr.AspNetCore` which retrieves the IP from `X-Forwarded-For` or `HttpContext.Connection.RemoteIpAddress`.
  3. Register it via DI extension methods in `Ali.PayTr.AspNetCore`.
  4. Inject `IPayTrClientIpAccessor` into `PayTrClient` and use it as a fallback instead of `"127.0.0.1"`.

### 2. Verify `FailedReasonCode` string compatibility
- **What:** Ensure that string-based error codes do not crash the webhook parser.
- **Why:** PayTR can sometimes emit error codes like `BLK`.
- **How:** The current `PayTrNotificationEndpoint` already uses `int.TryParse(form["failed_reason_code"].ToString(), out var failedReasonCode) ? failedReasonCode : -1`. This is safe and robust. No code change required.

## 3. Production Readiness Verdict
From an engineering perspective, the IFrame API is **fully implemented and ready for production use**. The code is resilient, defensively programmed, and accurately adheres to PayTR's strict hash calculation rules.

The IP Accessor fix is a minor enhancement that can be shipped before the stable 1.0.0 release.
