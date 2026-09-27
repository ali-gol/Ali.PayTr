# PayTR Direct API Business Analysis Report

## 1. Scope & Methodology
- **Analyzed Documentation:** `./docs/PayTR Direkt API` (Read Me First.md, STEP 1.md, STEP 2.md, and provided .NET Sample codes).
- **Analyzed Codebase:** `src/Ali.PayTr.Abstractions` (Models, Enums, Interfaces) and `src/Ali.PayTr.Core` (Clients, Services).
- **Focus:** Integration of "Direct API" alongside the existing IFrame-based implementations.

## 2. Endpoint & Feature Coverage Matrix
| Feature / Endpoint | Status | Notes |
|--------------------|--------|-------|
| Direct API Token Generation | ❌ Missing | Current token logic in `PayTrClient.cs` is strictly for the IFrame API. The hash logic differs significantly for Direct API. |
| Direct API Payment POST | ❌ Missing | Package currently posts to `odeme/api/get-token`. Direct API requires posting form data directly to `https://www.paytr.com/odeme`. |
| Direct API Sync Mode (`sync_mode`) | ❌ Missing | Synchronous API response (JSON) parsing is not supported. The system only handles redirect URLs currently. |
| Non-3D Transactions (`non_3d`) | ❌ Missing | Parameters and flow for Non-3D are entirely absent from current models. |
| Callback Notification (STEP 2) | ✅ Implemented | `PayTrNotificationProcessor` successfully covers hash verification and standard status updates which are consistent across both APIs. |

## 3. Parameter Gap Details
The following parameters are required by the PayTR Direct API spec but are entirely missing from `PayTrCreatePaymentRequest.cs`:

**Missing Credit Card & Flow Parameters:**
- `card_number` (string)
- `cc_owner` (string)
- `expiry_month` (string)
- `expiry_year` (string)
- `cvv` (string)
- `card_type` (string - advantage, axess, combo, bonus, cardfinans, maximum, paraf, world, saglamkart)

**Missing Configuration Parameters:**
- `non_3d` (0 or 1)
- `non3d_test_failed` (0 or 1)
- `sync_mode` (0 or 1)
- `request_exp_date` (int timestamp)
- `payment_type`: Direct API specifies `card` or `card_points`. In the current implementation, `payment_type` is not sent at all in the POST request.
- `client_lang`: Direct API documentation refers to this as `client_lang`, but the current package sends `lang` (line 106 in `PayTrClient.cs`).

**Token Hash Gap (`paytr_token`):**
- **Current (IFrame) Token Hash:** `merchant_id` + `user_ip` + `merchant_oid` + `email` + `payment_amount` + `user_basket` + `no_installment` + `max_installment` + `currency` + `test_mode` + `merchant_salt`
- **Required (Direct API) Token Hash:** `merchant_id` + `user_ip` + `merchant_oid` + `email` + `payment_amount` + `payment_type` + `installment_count` + `currency` + `test_mode` + `non_3d` + `merchant_salt`
*(Note: Direct API token hash deliberately omits basket details and uses exact installment counts rather than max/no installment rules).*

## 4. Business Rule Edge Cases
- **Amount Handling (CRITICAL RISK):** The current IFrame implementation in `PayTrClient.cs` multiplies the `PaymentAmount` by 100 to convert to kuruş (e.g., `100.99` becomes `10099`). The Direct API documentation explicitly expects the decimal format with dots (e.g., `100.99` or `150`). Reusing the current conversion logic will result in charging users 100x the intended amount.
- **Sync Mode vs. Redirect Flow:** Using `sync_mode=1` changes the response of the API completely. Instead of redirecting to `merchant_ok_url` or `merchant_fail_url`, PayTR returns a direct JSON response with states: `success`, `failed`, or `wait_callback`. The codebase currently has no support for handling this background processing pattern.
- **Callback Duplication:** When using Sync Mode, PayTR will still post to the Callback URL. `wait_callback` implies the transaction outcome depends solely on the webhook. `PayTrNotificationProcessor`'s idempotency controls (already present) are critical here to prevent double-processing.
- **Card Tokenization (Stored Cards):** Although not deeply featured in standard requests, Sync Mode responses can include `utoken` and `ctoken` for stored cards. The package currently ignores these fields.

## 5. Error Code Coverage
- **Coverage Status:** ✅ Excellent
- `PayTrFailReasonService.cs` correctly covers all the failed reason codes mentioned in the STEP 2 documentation (`0, 1, 2, 3, 6, 8, 9, 10, 11, 99`) with exact matches for the translated descriptions.

## 6. Findings Summary Table

| Priority | Finding | Actionable Implication |
|----------|---------|------------------------|
| **Critical** | Amount Format Discrepancy | Direct API expects decimal `100.99`, not kuruş `10099`. The existing `ConvertAmountToString` method will cause 100x overcharges if reused. |
| **Critical** | Token Hash Generation | Direct API requires a completely different parameter concatenation for the hash string. |
| **High** | Missing Model Parameters | `PayTrCreatePaymentRequest` lacks all sensitive card fields (`card_number`, `cvv`, etc.) needed for Direct API. |
| **High** | Endpoint & Method Incorrect | Need a separate API call routing to `https://www.paytr.com/odeme` with form data, instead of `odeme/api/get-token`. |
| **Medium** | Language Parameter Name | The parameter for language is `client_lang` in Direct API, but currently sent as `lang`. |
| **Medium** | Missing Sync Mode Support | No handler for synchronous JSON responses (`sync_mode=1`). |
