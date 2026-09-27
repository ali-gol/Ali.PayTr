# PayTR Business Analyst Analysis: IFrame API Audit

## 1. Scope & Methodology
This analysis evaluates the current implementation of the PayTR IFrame API (STEP 1 and STEP 2) in the NuGet package (`Ali.PayTr.Abstractions`, `Ali.PayTr.Core`, `Ali.PayTr.AspNetCore`). The audit compares the implementation against the official PayTR documentation in `./docs/PayTR IFrame API/` to identify missing features, gaps, and business rule edge cases.

## 2. Endpoint & Feature Coverage Matrix

| Feature / Step | Package Component | Status | Notes |
|---|---|---|---|
| STEP 1: Get Token | `PayTrClient.CreatePaymentAsync` | ✅ Implemented | Covers all required token request fields. |
| STEP 1: Hash Calculation | `PayTrHashService` & `PayTrClient` | ✅ Implemented | Hash concatenation exactly matches PayTR docs. |
| STEP 2: Receive Callback | `PayTrNotificationEndpoint` | ✅ Implemented | Exposes a Webhook endpoint to receive POST payload. |
| STEP 2: Hash Verification | `PayTrNotificationProcessor` | ✅ Implemented | Cryptographically compares received Hash vs Expected Hash. |
| STEP 2: Respond "OK" | `PayTrNotificationEndpoint` | ✅ Implemented | Responds `OK` (or `BadRequest` if validation fails). |
| STEP 2: Handle Duplicates | `PayTrNotificationProcessor` | ✅ Implemented | Deduplicates notifications to prevent replay processing. |

## 3. Parameter Gap Details

### STEP 1 (Token Request Parameters)
All required fields are currently accounted for in `PayTrCreatePaymentRequest` and properly mapped to the form-urlencoded dictionary in `PayTrClient.cs`.
- `user_ip`: Handled via `ClientIp` or defaults to `127.0.0.1`.
- `merchant_oid`: Mapped from `CorrelationId`.
- `user_basket`: Properly serialized to Base64 encoded JSON.
- `payment_amount`: Handled via `ConvertAmountToString`.
- `timeout_limit`, `debug_on`, `test_mode`, `currency`, `lang`, `no_installment`, `max_installment`: All managed correctly.

**Findings/Gaps:**
- `ClientIp`: The docstring for `ClientIp` in `PayTrCreatePaymentRequest` promises automatic IP extraction using `IHttpContextAccessor` if left null. Currently, `PayTrClient.cs` falls back to `127.0.0.1` and no automated extraction occurs. This is a behavioral gap compared to expectations.

### STEP 2 (Callback Notification Parameters)
All fields sent by PayTR are captured in `PayTrNotificationRequest` via `PayTrNotificationEndpoint`.
- `failed_reason_code` and `failed_reason_msg` are handled correctly.
- `total_amount` vs `payment_amount` are separated correctly as defined by PayTR docs.

**Findings/Gaps:**
- The parsing fallback for `FailedReasonCode` is `-1`. Since PayTR sends string-based codes like `BLK` for some APIs, this is technically a gap if we use this for all APIs, though for IFrame API specifically, codes are numeric.

## 4. Business Rule Edge Cases

- **Amount Handling:** The method `(long)Math.Round(amount * 100, MidpointRounding.AwayFromZero)` correctly accommodates the 100 multiplier and kuruş calculations (e.g. 34.56 -> 3456).
- **Duplicate Callback Notifications:** Handled by validating if the `OrderStatus` is already `CompletedWithSuccess` or `CompletedWithFail` and responding "OK" without triggering side-effects again.
- **Callback "OK" Response Rules:** The documentation explicitly forbids HTML or session checks. The `PayTrNotificationEndpoint` returns `Results.Text("OK", "text/plain")`, satisfying this.

## 5. Error Code Coverage
The `PayTrFailReasonService` maps the explicit numeric fail reason codes detailed in the PayTR Error Codes documentation precisely.

## 6. Findings Summary Table

| Finding | Priority | Description |
|---|---|---|
| IP Detection implementation missing | Medium | `PayTrCreatePaymentRequest` doc comments mention automatic IP detection using `IHttpContextAccessor` if `ClientIp` is null, but `PayTrClient` hardcodes "127.0.0.1" if null. |
| Potential 400 on valid edge-case retries | Low | Returning 400 on invalid payload is fine, but may confuse some integrations. |

The IFrame API integration covers the documented specifications perfectly. No major capabilities are missing for basic requirements.
