# PayTR IFrame API Audit — Architectural Review

## 1. Architectural Assessment

### What Works Well
The package follows a clean layered architecture with good separation of concerns:

```
Ali.PayTr.Abstractions (zero deps)  →  Public API surface
    ↓
Ali.PayTr.Core (Microsoft.Extensions.*)  →  Business logic, HTTP, hashing
    ↓
Ali.PayTr.EFCore (EF Core)  →  Persistence (opt-in)
Ali.PayTr.AspNetCore (ASP.NET Core)  →  Web integration (opt-in)
    ↓
Ali.PayTr (meta-package)  →  Convenience aggregation
```

**Strong points:**
- **Abstractions-first design**: `IPayTrClient`, `IPayTrHashService`, `IPayTrOrderService`, `IPayTrRefundService`, `IPayTrQueryService` allow consumers to mock/replace any layer.
- **Event-based webhook handling**: `IPayTrOrderEventHandler` pattern lets consumers react to payment events without modifying the core. `NullPayTrOrderEventHandler` is a nice no-op default.
- **HttpClientFactory integration**: All three HTTP services (`PayTrClient`, `PayTrRefundService`, `PayTrQueryService`) use `AddHttpClient<>`, avoiding socket exhaustion.
- **Repository pattern**: `IPayTrRepository` keeps persistence concerns out of the core business logic. EF Core implementation is opt-in.
- **Options pattern**: `PayTrOptions` with `[Required]` attributes and `ValidateDataAnnotations().ValidateOnStart()` ensures fail-fast on misconfiguration.

### What Could Be Better
- **No abstraction layer between PayTR's raw API and the package's domain model**: `PayTrClient` does both HTTP communication AND domain translation (amount conversion, hash building, basket serialization). These should be separate concerns.
- **`PayTrNotificationProcessor` has too many responsibilities**: Hash verification, order state management, notification logging, event dispatching, and fail reason enrichment — all in one 256-line class.
- **Service lifetimes are appropriate**: Singleton for stateless services (Hash, FailReason), Scoped for DB-dependent services (Order, Notification, EventDispatcher), HttpClient via factory — all correct.

---

## 2. Developer Experience (DX) & Abstraction Design

### PayTR Complexity Successfully Hidden
The package effectively isolates consumers from PayTR's quirks:
- **form-urlencoded** submissions → hidden behind `CreatePaymentAsync()`.
- **Base64(JSON(array))** basket format → consumers just provide `List<PayTrBasketItem>` with `Name`, `Price`, `Quantity`.
- **Amount × 100 integer conversion** → consumers provide `decimal PaymentAmount`, conversion is internal.
- **HMAC-SHA256 hash generation** → completely internal, consumers never see hash strings.
- **Plain-text "OK" webhook response** → handled by `PayTrNotificationEndpoint`, consumers implement `IPayTrOrderEventHandler`.
- **merchant_oid ↔ CorrelationId mapping** → consumers use `Guid CorrelationId`, `MerchantOidConverter` handles the translation.

### DX Concerns

**D1: `debug_on` is not independently configurable** (ref BA F2)
The `debug_on` flag controls whether PayTR displays error messages on the payment page. This is a debugging/integration tool. It should have its own property in `PayTrOptions` rather than being coupled to `TestMode`. Developers debugging production issues need `debug_on=1` with `test_mode=0`.

**Priority**: P0 — This is a production safety concern.

**D2: No fluent validation or guard clauses**
Request models (`PayTrCreatePaymentRequest`) use `required` keyword for compile-time checks but have zero runtime validation. PayTR silently rejects requests when parameter limits are exceeded (e.g., `user_name` > 60 chars). The developer gets an unhelpful generic error from PayTR.

**Priority**: P1 — Should validate before sending to PayTR and throw a clear `ArgumentException`.

**D3: `CorrelationId` defaults to `Guid.Empty`**
`PayTrCreatePaymentRequest.CorrelationId` defaults to `default!` (= `Guid.Empty`). If a consumer forgets to set it, the package will create a payment with `merchant_oid = "00000000000000000000000000000000"`. No guard clause prevents this.

**Priority**: P1 — Should throw on empty Guid.

---

## 3. Security Architecture

### Positive Security Posture
- **Timing-attack-safe hash comparison**: `CryptographicOperations.FixedTimeEquals` in `PayTrNotificationProcessor.cs` L64 — this is the correct approach for HMAC verification. ✅
- **HMAC-SHA256 key management**: `MerchantKey` flows through `IOptions<PayTrOptions>` → services. Standard .NET pattern. Keys are not hardcoded or logged at sensitive levels.
- **Webhook validates before processing**: Hash check happens before any order state mutation. ✅

### Security Concerns

**S1: Sensitive data in trace logs**
`PayTrOrderService.cs` L74 logs the PayTR API token at Trace level: `"PayTR API Token Success. Token: {response.Token}"`. While Trace is rarely enabled in production, the token value could leak into log aggregation systems. Similarly, `PayTrClient.cs` L100 logs the full API response at Trace.

**Recommendation**: Redact or mask token values in log output. Use structured logging with a `[SensitiveData]` attribute pattern or simply log `Token=***{last4}`.

**Priority**: P2

**S2: MerchantKey/MerchantSalt as plain strings in Options**
This is standard for the Options pattern and acceptable for a NuGet package — key management (Key Vault, env vars) is the consumer's responsibility. The package correctly does NOT prescribe how secrets are stored.

**Status**: Acceptable ✅

**S3: Notification endpoint has no rate limiting or IP filtering**
The `PayTrNotificationEndpoint` accepts POST from any IP. PayTR doesn't document a fixed set of callback IPs, so IP filtering is impractical. However, the hash verification provides adequate authentication.

**Status**: Acceptable ✅ — Hash verification is sufficient.

---

## 4. Resilience & Reliability

### Current State
- **HttpClient lifecycle**: Managed by `IHttpClientFactory` — no socket exhaustion risk. ✅
- **Network error handling**: `PayTrClient.cs` L128-137 catches `HttpRequestException` and `TaskCanceledException`, returning a failed response instead of throwing. ✅
- **No retry/timeout policies**: The three HTTP services (`PayTrClient`, `PayTrRefundService`, `PayTrQueryService`) have no Polly policies configured.

### Resilience Assessment

**R1: No configurable timeout**
The `HttpClient` uses the default 100-second timeout. For a payment token request, this is too long — the user would be staring at a loading screen. A 15-30 second timeout would be more appropriate.

**Recommendation**: Add `HttpClient.Timeout` configuration to `PayTrOptions` or set a reasonable default in the `AddHttpClient` configuration.

**Priority**: P2

**R2: No retry policy for token requests**
Transient network failures during `get-token` would immediately fail the checkout. A single retry with a short delay (e.g., 1 second) would improve reliability without degrading UX.

**However** — for a "basic iframe API" package targeting simplicity, adding Polly as a dependency may be over-engineering. Consumers can configure their own `HttpMessageHandler` pipeline via `AddHttpClient` + Polly themselves.

**Recommendation**: Document how to add retry policies rather than bundling Polly.

**Priority**: P3 — Nice to have, not blocking for initial release.

**R3: Webhook idempotency is solid**
The `wasFinalized` check in `PayTrNotificationProcessor.cs` L115-130 correctly prevents duplicate processing. Events are dispatched only once. Duplicate notifications are logged and acknowledged with "OK". ✅

---

## 5. Error Handling Strategy

### Current Pattern: Result Pattern ✅
The package consistently uses the Result pattern (`IsSuccess` + `Message`) rather than exceptions for expected business failures:
- `PayTrCreatePaymentResponse.IsSuccess` / `.Message`
- `PayTrRefundResponse.IsSuccess` / `.Message`
- `PayTrQueryResponse.IsSuccess` / `.Message`
- `PayTrNotificationVerifyResult.IsVerificationSuccessful` / `.FailureReason`

This is the right design for a payment SDK where "payment declined" is a normal business outcome, not an exceptional condition.

### Error Handling Concerns

**E1: Inconsistent error structure across APIs**
The Result pattern is applied inconsistently:
- `PayTrCreatePaymentResponse` uses `Message` for the error reason
- `PayTrRefundResponse` uses `Message` for `err_msg`
- `PayTrQueryResponse` uses `Message` for `err_msg` but also has `FailedReasonCode` / `FailedReasonMessage`

A unified base error model (e.g., `PayTrResult<T>` or a shared `ErrorCode` + `ErrorMessage` structure) would improve consistency.

**Priority**: P2 — Functional, but inconsistent.

**E2: No structured error codes for Refund/Query** (ref BA F5, F6)
The Refund and Query services return raw `err_msg` strings but don't expose `err_no`. Consumers cannot programmatically handle specific errors (e.g., "insufficient balance" vs "transaction too old").

**Recommendation**: Expose `ErrorCode` in `PayTrRefundResponse` and `PayTrQueryResponse`. Consider an enum for known error codes.

**Priority**: P1 for publish readiness — consumers need to differentiate between "retry later" and "never retry" errors.

**E3: PayTR API returns non-JSON responses sometimes**
If the PayTR API returns an HTML error page (server down, maintenance), `JsonDocument.Parse()` will throw. `PayTrRefundService` and `PayTrQueryService` catch `JsonException` ✅, but `PayTrClient` does not handle malformed responses — it would throw an unhandled `JsonException`.

Wait, looking again: `PayTrClient.cs` L128 catches `HttpRequestException` and `TaskCanceledException` but NOT `JsonException`. If PayTR returns an HTML error page, the JSON parsing at L101 would throw an unhandled exception.

**Priority**: P0 — This could crash the consumer's application.

---

## 6. Package Integrity & Compatibility

### Dependency Minimization ✅
External dependencies are minimal and appropriate:
- **Abstractions**: Zero external deps (pure .NET)
- **Core**: `Microsoft.Extensions.Http`, `Microsoft.Extensions.Logging.Abstractions`, `Microsoft.Extensions.Options`
- **AspNetCore**: `Microsoft.AspNetCore.Http.Abstractions`
- **EFCore**: `Microsoft.EntityFrameworkCore`

No third-party packages (Newtonsoft, Polly, etc.) — clean and maintainable.

### Target Framework
Should be verified from `.csproj` files, but the use of `CryptographicOperations.FixedTimeEquals`, `required` keyword, and collection expressions (`[]`) suggests .NET 8+.

### Breaking Change Assessment
The package is pre-1.0. Per SemVer, breaking changes are expected. For a first publish:
- The BA-identified bug (F1) must be fixed before publish.
- The `debug_on` issue (F2) should be fixed or at minimum documented.
- The `JsonException` gap (E3 above) must be fixed.

### Versioning Strategy
Recommend starting at `0.1.0` or `1.0.0-beta.1` to signal the package is functional but under active development.

---

## 7. Proposed Design Decisions Summary

| # | Decision | Rationale | Impact | Priority |
|---|---|---|---|---|
| **DD1** | Fix the notification enrichment bug (BA F1) | This is a data corruption bug that affects every single notification | Correctness | **P0** |
| **DD2** | Add `debug_on` as an independent option in `PayTrOptions` | Decouples debugging from test mode; prevents info leakage in production | Security, DX | **P0** |
| **DD3** | Add `JsonException` catch in `PayTrClient.CreatePaymentAsync()` | PayTR may return non-JSON (HTML error pages). Current code would throw unhandled. | Reliability | **P0** |
| **DD4** | Add guard clause for `CorrelationId == Guid.Empty` | Prevents creating payments with `merchant_oid = 00000...0` | Data integrity | **P1** |
| **DD5** | Add `ErrorCode` to `PayTrRefundResponse` and `PayTrQueryResponse` | Consumers need to programmatically differentiate error types | DX, Error handling | **P1** |
| **DD6** | Add basic input validation to `PayTrCreatePaymentRequest` | Validate string lengths, installment range before sending to PayTR | DX, Reliability | **P1** |
| **DD7** | Redact sensitive values (tokens, keys) in log output | Prevent accidental credential leakage in log aggregation | Security | **P2** |
| **DD8** | Add configurable `HttpClient.Timeout` | Default 100s is too long for payment flows | Reliability, UX | **P2** |
| **DD9** | Add English translations to error code dictionary | Current messages are Turkish-only | Internationalization | **P2** |
| **DD10** | Remove dead code (`PayTrFailedReasonDictionary`, `OrderPayTrNotificationDto`) | Reduces confusion and package size | Code quality | **P3** |
| **DD11** | Document how to add Polly retry policies | Rather than bundling Polly, show consumers how to opt-in | DX | **P3** |

### Production Readiness Verdict (Architectural Perspective)

**After fixing P0 items (DD1, DD2, DD3), the package is architecturally sound for basic IFrame API production use.**

The layered architecture, HttpClientFactory usage, timing-safe hash verification, webhook idempotency, and event-based extensibility model are all well-designed. The package successfully abstracts PayTR's complexity behind a clean, idiomatic .NET API.

For "basic and simple needs" (IFrame payment + webhook + optional refund/query), the architecture is appropriate and not over-engineered. The missing APIs (Direct, CAPI, BIN, etc.) can be added incrementally without breaking changes.
