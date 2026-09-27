# PayTR IFrame API Audit — Engineering Plan

## 1. Code-Level Audit Findings

| # | File | Line(s) | Issue | Severity | Category |
|---|---|---|---|---|---|
| **E1** | `src/Ali.PayTr.Core/Services/PayTrNotificationProcessor.cs` | 82-87 | **BUG: Inverted condition** — `if (notification.IsSuccess)` enriches fail reasons on SUCCESS instead of FAILURE. Failed payments don't get description enrichment; successful payments get corrupted with error code 0 data. | 🔴 Critical | API Contract Violation |
| **E2** | `src/Ali.PayTr.Core/Clients/PayTrClient.cs` | 96-137 | **Missing `JsonException` catch** — If PayTR API returns non-JSON (HTML error page, 503, maintenance page), `JsonDocument.Parse()` at L101 throws unhandled `JsonException`, bypassing the existing catch for network errors. | 🔴 Critical | Missing Validation |
| **E3** | `src/Ali.PayTr.Core/Clients/PayTrClient.cs` | 82 | `debug_on` hardcoded to `TestMode` value. In production (`TestMode=false`), `debug_on=0`. Cannot debug production integration issues without switching to test transactions. | 🟠 High | Security Vulnerability |
| **E4** | `src/Ali.PayTr.Core/Clients/PayTrClient.cs` | 76 | `user_ip` falls back to `"127.0.0.1"`. PayTR uses IP for fraud detection. Sending localhost IP in production could flag or reject legitimate transactions. | 🟡 Medium | Missing Validation |
| **E5** | `src/Ali.PayTr.Core/Services/PayTrNotificationProcessor.cs` | 45 | `Guid.TryParse` already handles malformed OID gracefully (returns `IsVerificationSuccessful=false`). ✅ No issue here — the code is correct. | ✅ No Issue | — |
| **E6** | `src/Ali.PayTr.Core/Services/PayTrOrderService.cs` | 74 | Token value logged at Trace level: `"PayTR API Token Success. Token: {response.Token}"`. While Trace is rarely enabled in production, the token could leak to log aggregation. | 🟡 Medium | Security Vulnerability |
| **E7** | `src/Ali.PayTr.Core/Clients/PayTrClient.cs` | 100 | Full API response logged at Trace: `$"PayTR api response received: {request.CorrelationId}\n{responseString}"`. Could contain sensitive data. | 🟡 Medium | Security Vulnerability |
| **E8** | `src/Ali.PayTr.Core/Clients/PayTrClient.cs` | 32 | `ConvertAmountToString` uses `MidpointRounding.AwayFromZero` — this is **CORRECT**. Standard banker's rounding (`ToEven`) would cause 1-kuruş discrepancies. ✅ | ✅ No Issue | — |
| **E9** | `src/Ali.PayTr.Core/Utilities/PayTrFailedReasonDictionary.cs` | 1-11 | Empty class. Dead code. | 🟢 Low | Code Quality |
| **E10** | `src/Ali.PayTr.Abstractions/Models/OrderPayTrNotificationDto.cs` | 1-16 | Snake_case DTO appears unused by any service. Dead code. | 🟢 Low | Code Quality |
| **E11** | `src/Ali.PayTr.Abstractions/Models/Records.cs` | 3 | `PayTrFeedbackFailedReasonItem` uses snake_case properties (`failed_reason_code`, `failed_reason_msg`). Inconsistent with .NET naming conventions. | 🟢 Low | Code Quality |

---

## 2. Architectural Objections (Pragmatism Check)

| Architect Decision | Engineer Verdict | Rationale |
|---|---|---|
| **DD1**: Fix notification enrichment bug | ✅ **Endorsed** | This is a straightforward 1-character fix (`!`). No debate. |
| **DD2**: Add `debug_on` as independent option | ✅ **Endorsed** | Simple: add `bool DebugMode` to `PayTrOptions`, default to `false`. Use it in `PayTrClient`. |
| **DD3**: Add `JsonException` catch in `PayTrClient` | ✅ **Endorsed** | Follow the same pattern as `PayTrRefundService` and `PayTrQueryService` which already catch `JsonException`. Consistency fix. |
| **DD4**: Guard clause for `CorrelationId == Guid.Empty` | ✅ **Endorsed with caveat** | Good for safety, but don't throw — return a failed response with a clear message. Consumers might not handle `ArgumentException` from a payment method. |
| **DD5**: Add `ErrorCode` to Refund/Query responses | ⚠️ **Defer to v1.1** | For "basic iframe API" v1, the refund/query services are bonuses. Exposing `err_no` as a string property is sufficient. No need for an enum yet. |
| **DD6**: Input validation on request models | ⚠️ **Simplified approach** | Don't validate every field. Just validate: (1) `CorrelationId != Guid.Empty`, (2) `PaymentAmount > 0`, (3) `BasketItems` not empty. Let PayTR validate the rest — it returns clear errors with `debug_on=1`. |
| **DD7**: Redact sensitive values in logs | ⚠️ **Defer to v1.1** | These are Trace-level logs. Pragmatically, this is low-risk for v1. Document that consumers should not enable Trace in production. |
| **DD8**: Configurable HttpClient timeout | ⚠️ **Defer** | Consumers can configure this via `AddHttpClient` pipeline. Don't add options bloat. |
| **DD9**: English translations for error codes | ⚠️ **Defer to v1.1** | The fail reason messages come from PayTR in Turkish. Adding English translations is a DX improvement but not blocking. |
| **DD10**: Remove dead code | ✅ **Endorsed** | Quick cleanup. |
| **DD11**: Document Polly integration | ⚠️ **Strongly endorsed over bundling Polly** | Polly adds a dependency and complexity. For a payment SDK, automatic retries on token requests are dangerous (orphaned tokens, double charges). Document how consumers can add retry policies themselves. |

### Summary: What to ship in v1.0

**Fix these 3 things and ship:**
1. E1: Fix the `!` in notification processor (1-char fix)
2. E2: Add `JsonException` catch in `PayTrClient` (5-line addition)
3. E3: Add `DebugMode` option and decouple from `TestMode` (10-line change)

**Nice to have for v1.0 but not blocking:**
4. DD4: Guard clause for `CorrelationId`
5. DD6: Basic validation (`PaymentAmount > 0`, `BasketItems` not empty)
6. DD10: Remove dead code

---

## 3. Implementation Change Plan

| # | File | Class/Method | What Changes | Why | Priority | Complexity |
|---|---|---|---|---|---|---|
| **C1** | `PayTrNotificationProcessor.cs` | `ProcessNotificationAsync` | Change L82 from `if (notification.IsSuccess)` to `if (!notification.IsSuccess)` | Fixes data corruption bug (E1, BA F1) | **P0** | Simple |
| **C2** | `PayTrClient.cs` | `CreatePaymentAsync` | Add `catch (JsonException)` alongside existing `HttpRequestException` catch, return failed `PayTrCreatePaymentResponse` with message "Invalid response format from PayTR" | Prevents crash on non-JSON responses (E2, DD3) | **P0** | Simple |
| **C3** | `PayTrOptions.cs` | `PayTrOptions` | Add `public bool DebugMode { get; set; } = false;` property | Decouples debug_on from test_mode (E3, DD2) | **P0** | Simple |
| **C4** | `PayTrClient.cs` | `CreatePaymentAsync` | Change L82 from `_options.TestMode ? "1" : "0"` to `_options.DebugMode ? "1" : "0"` | Uses the new independent option (E3, DD2) | **P0** | Simple |
| **C5** | `PayTrClient.cs` | `CreatePaymentAsync` | Add guard: `if (request.CorrelationId == Guid.Empty) return new PayTrCreatePaymentResponse { IsSuccess = false, Message = "CorrelationId must be set" };` | Prevents 00000...0 merchant_oid (DD4) | **P1** | Simple |
| **C6** | `PayTrClient.cs` | `CreatePaymentAsync` | Add guard: `if (request.PaymentAmount <= 0) return new ...` and `if (!request.BasketItems.Any()) return new ...` | Basic sanity checks (DD6) | **P1** | Simple |
| **C7** | `PayTrFailedReasonDictionary.cs` | — | Delete the file | Dead code removal (E9, DD10) | **P3** | Simple |
| **C8** | `OrderPayTrNotificationDto.cs` | — | Delete the file if confirmed unused | Dead code removal (E10, DD10) | **P3** | Simple |

---

## 4. Unit Test Plan

### PayTrNotificationProcessor Tests

| Test | Verifies | Arrange/Act/Assert |
|---|---|---|
| `ProcessNotification_FailedPayment_EnrichesFailReason` | Bug fix E1 — fail reason enrichment runs on failures | Arrange: notification with `Status="failed"`, `FailedReasonCode=6`. Act: process. Assert: `FailedReasonMsg` and `FailedReasonDescription` are populated from dictionary. |
| `ProcessNotification_SuccessfulPayment_DoesNotEnrichFailReason` | Bug fix E1 — success payments don't get error data | Arrange: notification with `Status="success"`. Act: process. Assert: `FailedReasonMsg` is NOT overwritten with code-0 message. |
| `ProcessNotification_DuplicateNotification_IsIdempotent` | Idempotency | Arrange: order already finalized. Act: send same notification again. Assert: DB not updated, events not dispatched, "OK" still returned. |

### PayTrClient Tests

| Test | Verifies | Arrange/Act/Assert |
|---|---|---|
| `CreatePayment_PayTrReturnsHtml_ReturnsFailedResult` | JsonException handling (E2) | Arrange: mock HttpClient returns `<html>503</html>`. Act: `CreatePaymentAsync()`. Assert: `IsSuccess=false`, no exception thrown. |
| `CreatePayment_DebugModeTrue_SendsDebugOn1` | DebugMode decoupling (E3) | Arrange: `DebugMode=true`, `TestMode=false`. Act: create payment. Assert: POST contains `debug_on=1` and `test_mode=0`. |
| `CreatePayment_DebugModeFalse_SendsDebugOn0` | DebugMode decoupling (E3) | Arrange: `DebugMode=false`. Act: create payment. Assert: POST contains `debug_on=0`. |
| `CreatePayment_EmptyCorrelationId_ReturnsFailedResult` | Guard clause (C5) | Arrange: `CorrelationId = Guid.Empty`. Act: create. Assert: `IsSuccess=false`. |
| `CreatePayment_ZeroAmount_ReturnsFailedResult` | Guard clause (C6) | Arrange: `PaymentAmount=0`. Act: create. Assert: `IsSuccess=false`. |
| `ConvertAmountToString_MidpointValue_RoundsAwayFromZero` | Existing correct behavior (E8) | Arrange: `34.565m`. Act: convert. Assert: result is `"3457"` (not `"3456"`). |

### PayTrFailReasonService Tests

| Test | Verifies |
|---|---|
| `GetFailedReason_AllDocumentedCodes_ReturnCorrectMessages` | All 10 codes (0,1,2,3,6,8,9,10,11,99) return non-null messages |
| `GetFailedReason_UnknownCode_ReturnsFallback` | Code 999 returns "Bilinmeyen Hata" fallback |
