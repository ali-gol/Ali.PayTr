# PayTR IFrame API Audit — Business Analyst Report

## 1. Scope & Methodology

### Analyzed Scope
- **Primary focus**: PayTR IFrame API (STEP 1: Token Generation, STEP 2: Callback/Webhook) — the stated goal of the package.
- **Secondary scope**: Refund API, Status Query API — implemented as supplementary features.
- **Out of scope (NOT implemented)**: Direct API, Card Storage (CAPI), BIN Service, Installment Rate Query, Transaction Detail/Reporting API, Platform Transfer.

### Documentation Consulted
| Doc Folder | Purpose |
|---|---|
| `docs/PayTR IFrame API/STEP 1/STEP 1.md` | Token request parameters, hash order |
| `docs/PayTR IFrame API/STEP 1/STEP 1 .NET Sample/iframe_example.aspx.cs` | Reference hash concatenation order verification |
| `docs/PayTR IFrame API/STEP 2/STEP 2.md` | Callback parameters, hash verification, error codes |
| `docs/PayTR IFrame API/STEP 2/STEP 2 .NET Sample/callback_url_sample.aspx.cs` | Reference callback hash verification |
| `docs/PayTR Error Codes.md` | Full error code catalog across all APIs |
| `docs/PayTR İade API/Paytr Refund API.md` | Refund endpoint spec |
| `docs/PayTR Mağaza Durum Sorgulama/PayTR Status Inquiry Service.md` | Status query spec |
| `docs/PayTR Direkt API/` | Direct API (not implemented, noted) |
| `docs/PayTR Kart Saklama API/` | Card Storage API (not implemented, noted) |
| `docs/PayTR BIN Sorgulama Servisi/` | BIN Service (not implemented, noted) |
| `docs/PayTR Taksit Oranları Sorgulama Servisi/` | Installment Service (not implemented, noted) |
| `docs/PayTR İşlem Dökümü API/` | Transaction Detail (not implemented, noted) |

### Codebase Analyzed
- `src/Ali.PayTr.Abstractions/` — Interfaces, Models, Enums, Events, Options
- `src/Ali.PayTr.Core/` — Service implementations, Clients, Entities, Utilities
- `src/Ali.PayTr.AspNetCore/` — ASP.NET Core endpoint mapping
- `src/Ali.PayTr.EFCore/` — Entity Framework configurations
- `tests/Ali.PayTr.Tests/` — Unit tests

---

## 2. Endpoint & Feature Coverage Matrix

### IFrame API Coverage (Primary Scope)

| PayTR IFrame API Feature | Status | Package Location | Notes |
|---|---|---|---|
| **STEP 1: Get iframe_token** | ✅ Implemented | `PayTrClient.CreatePaymentAsync()` | POSTs to `odeme/api/get-token`, returns token + iframe URL |
| **STEP 1: Hash generation** | ✅ Implemented | `PayTrClient.cs` L57-69 | Concatenation order matches docs exactly |
| **STEP 1: Amount conversion (×100)** | ✅ Implemented | `PayTrClient.ConvertAmountToString()` | `(long)Math.Round(amount * 100, AwayFromZero)` |
| **STEP 1: Basket Base64 JSON** | ✅ Implemented | `PayTrClient.cs` L51-52 | Serializes `[name, price, quantity]` arrays to JSON then Base64 |
| **STEP 1: Installment control** | ✅ Implemented | `PayTrClient.cs` L54-55 | `no_installment`/`max_installment` derived from `InstallmentCount` |
| **STEP 1: Currency** | ✅ Implemented | `PayTrCreatePaymentRequest.Currency` | Default `"TRY"` |
| **STEP 1: Language** | ✅ Implemented | Request `Language` + Options `Language` fallback | Default `"tr"` |
| **STEP 1: Timeout limit** | ✅ Implemented | Request `TimeoutLimitMinutes` + Options fallback | Default 30 min |
| **STEP 1: debug_on** | ⚠️ Partial | `PayTrClient.cs` L82 | **Tied to TestMode** — should be independent |
| **STEP 2: Callback endpoint** | ✅ Implemented | `PayTrNotificationEndpoint.HandleAsync()` | Maps to `/{routePrefix}/notification` |
| **STEP 2: Hash verification** | ✅ Implemented | `PayTrNotificationProcessor.cs` L57-64 | Uses `CryptographicOperations.FixedTimeEquals` ✅ |
| **STEP 2: "OK" response** | ✅ Implemented | `PayTrNotificationEndpoint.cs` L51 | `Results.Text("OK", "text/plain")` |
| **STEP 2: Duplicate notification handling** | ✅ Implemented | `PayTrNotificationProcessor.cs` L115-130 | Checks `wasFinalized` before updating |
| **STEP 2: Error code mapping** | ✅ Implemented | `PayTrFailReasonService.cs` | All 10 IFrame callback codes mapped |
| **STEP 2: Notification logging** | ✅ Implemented | `PayTrNotificationProcessor.LogNotificationAsync()` | Full raw body persisted |
| **STEP 2: Event dispatching** | ✅ Implemented | `PayTrOrderEventDispatcher` | Success/Failure events to registered `IPayTrOrderEventHandler` |
| **Order lifecycle management** | ✅ Implemented | `PayTrOrderService.CreateOrderAndGetPaymentUrlAsync()` | Creates order in DB, calls API, logs status changes |

### Other API Coverage

| PayTR API | Status | Notes |
|---|---|---|
| Refund API (`/odeme/iade`) | ✅ Implemented | `PayTrRefundService.RefundAsync()` |
| Status Query API (`/odeme/durum-sorgu`) | ✅ Implemented | `PayTrQueryService.QueryStatusAsync()` |
| Direct API (card-on-merchant-side) | ❌ Missing | Not in scope per stated goals |
| Card Storage API (CAPI) | ❌ Missing | Tokenization, list, delete, recurring payments |
| BIN Service | ❌ Missing | Card bin lookup |
| Installment Rate Query | ❌ Missing | Merchant installment rate query |
| Transaction Detail/Reporting | ❌ Missing | Sales/return transaction reports |
| Platform Transfer (Marketplace) | ❌ Missing | Sub-merchant transfer requests |

---

## 3. Parameter Gap Details

### 3.1 IFrame Token Request (`get-token`)

| Parameter | Docs Type | Package Implementation | Status | Issue |
|---|---|---|---|---|
| `merchant_id` | integer | `string` (`PayTrOptions.MerchantId`) | ⚠️ Type mismatch | Docs say integer, package sends as string. PayTR likely accepts both, but type is inconsistent. |
| `user_ip` | string (≤39 chars) | `request.ClientIp ?? "127.0.0.1"` | ⚠️ Risky fallback | Falls back to `127.0.0.1` if IP accessor fails. PayTR may reject or flag. |
| `merchant_oid` | string (≤64 chars, alphanumeric) | `Guid.ToString("N")` = 32 hex chars | ✅ | Within limits, alphanumeric |
| `email` | string (≤100 chars) | `request.CustomerEmail` | ✅ | No length validation |
| `payment_amount` | integer (×100) | `(long)Math.Round(amount * 100)` | ✅ | Correct |
| `user_basket` | string (Base64 JSON) | Base64(JSON array) | ✅ | Correct format |
| `no_installment` | int (0 or 1) | Derived from `InstallmentCount` | ✅ | |
| `max_installment` | int (0,2-12) | Derived from `InstallmentCount` | ✅ | No validation that value is in allowed set |
| `currency` | string | `request.Currency` default "TRY" | ⚠️ Minor | Docs default to "TL". Both TL and TRY are accepted. |
| `paytr_token` | string | HMAC-SHA256 Base64 | ✅ | Hash order matches docs |
| `user_name` | string (≤60 chars) | `request.CustomerFullName` | ✅ | No length validation |
| `user_address` | string (≤400 chars) | `request.CustomerAddress` | ✅ | No length validation |
| `user_phone` | string (≤20 chars) | `request.CustomerPhone` | ✅ | No length validation |
| `merchant_ok_url` | string (≤400 chars) | Built from `SuccessUrlPattern` | ✅ | |
| `merchant_fail_url` | string (≤400 chars) | Built from `FailUrlPattern` | ✅ | |
| `test_mode` | int (0 or 1) | From `PayTrOptions.TestMode` | ✅ | |
| `debug_on` | int (0 or 1) | **Hardcoded to TestMode value** | ⚠️ | Should be separately configurable |
| `timeout_limit` | int (minutes) | From request or options (default 30) | ✅ | |
| `lang` | string (tr/en) | From request or options (default "tr") | ✅ | |

### 3.2 Callback Notification Fields

| Field | Docs | Package | Status |
|---|---|---|---|
| `merchant_oid` | ✓ (success+failed) | `PayTrNotificationRequest.MerchantOid` | ✅ |
| `status` | ✓ (success+failed) | `PayTrNotificationRequest.Status` | ✅ |
| `total_amount` | ✓ (success+failed) | `PayTrNotificationRequest.TotalAmount` | ✅ |
| `hash` | ✓ (success+failed) | `PayTrNotificationRequest.Hash` | ✅ |
| `failed_reason_code` | failed only | `PayTrNotificationRequest.FailedReasonCode` | ✅ |
| `failed_reason_msg` | failed only | `PayTrNotificationRequest.FailedReasonMsg` | ✅ |
| `test_mode` | ✓ (success+failed) | `PayTrNotificationRequest.TestMode` | ✅ |
| `payment_type` | ✓ (success+failed) | `PayTrNotificationRequest.PaymentType` | ✅ |
| `currency` | success only | `PayTrNotificationRequest.Currency` | ✅ |
| `payment_amount` | success only | `PayTrNotificationRequest.PaymentAmount` | ✅ |

### 3.3 Refund API Parameters

| Parameter | Docs Type | Package | Status |
|---|---|---|---|
| `merchant_id` | integer | `_options.MerchantId` (string) | ⚠️ Same type note |
| `merchant_oid` | string | `MerchantOidConverter.ToMerchantOid()` | ✅ |
| `return_amount` | integer (docs say integer but example shows decimal with period) | `F2` with InvariantCulture | ✅ Matches example |
| `paytr_token` | string | HMAC-SHA256 Base64 | ✅ |
| `reference_no` | string (≤64 chars, optional) | `PayTrRefundRequest.ReferenceNo` | ✅ |

### 3.4 Status Query Parameters
All parameters match the docs specification. ✅

---

## 4. Business Rule Edge Cases

### 4.1 🐛 BUG: Failed Reason Enrichment Logic is Inverted

**File**: `src/Ali.PayTr.Core/Services/PayTrNotificationProcessor.cs` Lines 82-87

```csharp
if (notification.IsSuccess)  // ← BUG: condition is inverted
{
    var failedReason = _payTrFailReasonService.GetFailedReasonByReasonCode(notification.FailedReasonCode);
    notification.FailedReasonMsg = failedReason.failed_reason_msg;
    notification.FailedReasonDescription = failedReason.description;
}
```

Per PayTR STEP 2 docs, `failed_reason_code` and `failed_reason_msg` are **only sent when status is "failed"**. This code enriches fail reason details only when `IsSuccess == true` (i.e., `status == "success"`), which means:
- On **failed** payments: The fail reason from PayTR's POST is used as-is without enrichment from the dictionary — the description field stays null.
- On **successful** payments: The code tries to look up a fail reason code that doesn't exist (defaults to 0), and **overwrites** the notification's `FailedReasonMsg` with the code-0 Turkish message. This is incorrect behavior for a successful payment.

**Severity**: 🔴 Critical — This is a functional bug that corrupts notification data.

### 4.2 Amount Handling
- **Kuruş conversion**: Correctly uses `(long)Math.Round(amount * 100, MidpointRounding.AwayFromZero)` — avoids floating-point truncation. ✅
- **Refund amount**: Uses `F2` format with `InvariantCulture` — period as decimal separator. Matches docs. ✅
- **Query response**: Correctly divides `payment_amount` by 100 when parsing query response. ✅

### 4.3 Installment Restrictions
- When `InstallmentCount == 1`: sends `no_installment=1, max_installment=0`. ✅
- When `InstallmentCount > 1`: sends `no_installment=0, max_installment={count}`. ✅
- **No validation** that `InstallmentCount` is within allowed values (0,2,3,4,...,12). A value like 13 or -1 would be sent to PayTR and rejected.

### 4.4 Webhook/Notification Retry Behavior
- The package correctly handles duplicate notifications by checking `wasFinalized` before updating order status. ✅
- On duplicate, it still logs the notification and responds with "OK". ✅
- However, it does NOT re-dispatch events for duplicates (correct per docs — only first notification should be processed). ✅

### 4.5 `debug_on` Tied to `TestMode`
**File**: `src/Ali.PayTr.Core/Clients/PayTrClient.cs` Line 82

`debug_on` is set to `_options.TestMode ? "1" : "0"`. Per PayTR docs, `debug_on` controls whether **error messages are displayed on the payment page** to end users. This is an integration debugging tool, not a test mode toggle. In production, if a developer wants to temporarily debug an issue, they cannot enable `debug_on` without switching the entire payment system to test mode (which uses test transactions, not real ones).

### 4.6 Currency Default Mismatch
- `PayTrCreatePaymentRequest.Currency` defaults to `"TRY"`.
- `PayTrOptions.Currency` defaults to `"TRY"`.
- PayTR docs state: _"TL (or TRY), EUR, USD, GBP, RUB (TL is assumed if not sent)"_.
- While `TRY` is accepted, the docs consistently use `TL` as the default. Minor but worth noting.

### 4.7 IP Address Fallback
- `PayTrClient.cs` L76: `userIp ?? "127.0.0.1"` — If the IP accessor is not registered or fails, the package sends `127.0.0.1` to PayTR. PayTR uses this for fraud checks; a localhost IP could cause transaction rejection or fraud flagging in production.

### 4.8 MerchantOid Format
- The package uses `Guid.ToString("N")` (32 hex chars without hyphens). This is within the 64-char alphanumeric limit.
- However, the `PayTrNotificationProcessor` at line 45 uses `Guid.TryParse(notification.MerchantOid, ...)` which means it can ONLY process notifications for orders created through this package. If a merchant_oid was set externally (not a Guid), the notification would be rejected as "Invalid MerchantOid format". This is by design but is a limitation.

### 4.9 Test Mode vs Production Mode
- `TestMode` is a global option in `PayTrOptions`, not per-request. This means switching between test and production requires a configuration change + restart. Acceptable for most use cases. ✅

---

## 5. Error Code Coverage

### 5.1 IFrame Callback Error Codes (Payment Failure Codes)

| Code | Docs Description (EN) | Package Mapping | Status |
|---|---|---|---|
| 0 | Variable — detailed error from bank | ✅ `PayTrFailReasonService` code 0 | ✅ Mapped |
| 1 | Authentication not performed | ✅ code 1 | ✅ Mapped |
| 2 | Authentication failed | ✅ code 2 | ✅ Mapped |
| 3 | Security check failed | ✅ code 3 | ✅ Mapped |
| 6 | Customer left/timed out | ✅ code 6 | ✅ Mapped |
| 8 | Installment not allowed for card | ✅ code 8 | ✅ Mapped |
| 9 | No authorization for card | ✅ code 9 | ✅ Mapped |
| 10 | 3D Secure required | ✅ code 10 | ✅ Mapped |
| 11 | Fraud alert | ✅ code 11 | ✅ Mapped |
| 99 | Technical integration error | ✅ code 99 | ✅ Mapped |

**All IFrame callback error codes are mapped. ✅**

### 5.2 Error Code Mapping Quality

- All messages in `PayTrFailReasonService` are in **Turkish only**. No English translations are provided. The docs include both Turkish and English versions. For an English-speaking developer or multi-language application, this limits usability.
- The `PayTrFeedbackFailedReasonItem` record uses snake_case properties (`failed_reason_code`, `failed_reason_msg`) — inconsistent with .NET conventions.
- The `PayTrFailedReasonDictionary` class in `Utilities/` is **empty** — appears to be an unused placeholder.

### 5.3 Refund API Error Codes

The docs define 12 refund-specific error codes (`err_no` 000-011 + BLK). **None of these are mapped in the package.** The `PayTrRefundService` reads `err_msg` from the response but does not provide structured error codes.

| Refund Error Code | In Package? |
|---|---|
| 000 - Refund locked | ❌ |
| 001 - Invalid request / inactive merchant | ❌ |
| 002 - Invalid merchant_oid | ❌ |
| 003 - Invalid return_amount | ❌ |
| 004 - Invalid paytr_token | ❌ |
| 005 - No successful payment found | ❌ |
| 007 - Payment not yet notified | ❌ |
| 008 - Payment type doesn't support refund | ❌ |
| 009 - Refund exceeds payment amount | ❌ |
| 010 - Insufficient balance | ❌ |
| 011 - Transaction older than 1 year | ❌ |
| BLK - Blocked transaction | ❌ |

### 5.4 Status Query Error Codes

| Query Error Code | In Package? |
|---|---|
| 001 - Invalid request / inactive merchant | ❌ |
| 002 - Invalid merchant_oid | ❌ |
| 003 - Invalid paytr_token | ❌ |
| 004 - Transaction/payment not found | ❌ |

The `PayTrQueryService` reads `err_msg` but does not expose `err_no` in the response model.

---

## 6. Findings Summary Table

| # | Severity | Finding | Location | Impact |
|---|---|---|---|---|
| **F1** | 🔴 **Critical** | **BUG: Failed reason enrichment logic is inverted** — `if (notification.IsSuccess)` should be `if (!notification.IsSuccess)`. Successful payments get corrupted with error code 0 message; failed payments don't get enriched descriptions. | `PayTrNotificationProcessor.cs` L82-87 | Data corruption on every notification |
| **F2** | 🟠 **High** | `debug_on` is tied to `TestMode` instead of being independently configurable. Cannot debug production issues without switching to test transactions. | `PayTrClient.cs` L82 | Cannot debug in production without using test mode |
| **F3** | 🟠 **High** | No input validation on request models. No length checks on `CustomerEmail` (≤100), `CustomerFullName` (≤60), `CustomerAddress` (≤400), `CustomerPhone` (≤20), `InstallmentCount` (allowed: 0,2-12). PayTR will reject silently or show errors. | `PayTrCreatePaymentRequest.cs` | Silent failures with cryptic PayTR errors |
| **F4** | 🟡 **Medium** | `PayTrFailReasonService` messages are Turkish-only. No English translations available. | `PayTrFailReasonService.cs` | Non-Turkish developers/users see untranslatable error messages |
| **F5** | 🟡 **Medium** | Refund API error codes (000-011, BLK) are NOT mapped. `PayTrRefundResponse` only exposes raw `err_msg` string. | `PayTrRefundService.cs` | No structured error handling for refund failures |
| **F6** | 🟡 **Medium** | Status Query error codes (001-004) are NOT mapped. `PayTrQueryResponse` doesn't expose `err_no`. | `PayTrQueryService.cs` | No structured error handling for query failures |
| **F7** | 🟡 **Medium** | `user_ip` falls back to `"127.0.0.1"` if IP accessor is not registered or returns null. | `PayTrClient.cs` L76, `PayTrOrderService.cs` L32 | PayTR fraud detection may flag or reject transactions |
| **F8** | 🟢 **Low** | `PayTrFailedReasonDictionary` class is empty/unused. Dead code. | `Utilities/PayTrFailedReasonDictionary.cs` | Code clutter |
| **F9** | 🟢 **Low** | `PayTrFeedbackFailedReasonItem` uses snake_case properties (`failed_reason_code`, `failed_reason_msg`). Inconsistent with .NET conventions. | `Records.cs` | Developer experience |
| **F10** | 🟢 **Low** | `OrderPayTrNotificationDto` class in Abstractions uses snake_case properties and appears unused by any service. Potential dead code. | `OrderPayTrNotificationDto.cs` | Code clutter |
| **F11** | 🟢 **Low** | Currency default `"TRY"` while PayTR docs default to `"TL"`. Both accepted, but inconsistent with docs. | `PayTrCreatePaymentRequest.cs` L28 | Cosmetic |
