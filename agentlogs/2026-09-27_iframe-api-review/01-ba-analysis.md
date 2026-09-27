# PayTR IFrame API Gap Analysis & Business Rule Audit

## 1. Scope & Methodology

**Scope:** Analysis of the PayTR IFrame API integration within the `Ali.PayTr.Abstractions`, `Ali.PayTr.Core`, and `Ali.PayTr.AspNetCore` projects.
**Methodology:** The existing codebase was compared directly against the official documentation located in `./docs/PayTR IFrame API/` (specifically `STEP 1` and `STEP 2`).

## 2. Endpoint & Feature Coverage Matrix

| PayTR API Capability | Status | Notes |
| :--- | :--- | :--- |
| STEP 1: IFrame Token Generation (`/odeme/api/get-token`) | ⚠️ Partial | Implemented, but contains critical flaws in parameter mappings. |
| STEP 2: Callback URL Handler | ⚠️ Partial | Implemented, but has a missing parameter and potential response format risk. |

## 3. Parameter Gap Details

### Token Generation (`/odeme/api/get-token`)
*   **`no_installment`:** Logic is severely inverted. The current implementation in `PayTrClient.cs` sends `"0"` (show installments) if `request.InstallmentCount == 1`, and `"1"` (hide installments) if `request.InstallmentCount > 1`. This means when a user explicitly requests multiple installments, the system instructs PayTR to hide the installment options.
*   **`max_installment`:** The implementation sends `request.InstallmentCount.ToString()`. If `InstallmentCount` is `1`, it sends `"1"`. However, the PayTR specification restricts this field strictly to: `0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12`. Sending `1` is an invalid value and may result in an API rejection.
*   **`timeout_limit`:** This field is currently hardcoded to `"30"` in `PayTrClient.cs`. It is missing from `PayTrCreatePaymentRequest` and `PayTrOptions`, providing no flexibility for merchants who want different session durations.
*   **`user_basket`:** The `Price` property of `PayTrBasketItem` is a `decimal`. Serializing it directly results in JSON numbers (e.g., `18.5`). The PayTR documentation samples strongly imply it should be a formatted string (e.g., `"18.50"`). Relying on default decimal serialization is a risk.

### Callback URL (Webhook Endpoint)
*   **`payment_amount`:** This field is sent in the STEP 2 POST request by PayTR (representing the original amount without installment differences), but it is **missing** from `PayTrNotificationRequest` and is completely ignored in `PayTrNotificationEndpoint.cs`.
*   **Endpoint Response:** The callback endpoint uses `Results.Ok("OK")`. In ASP.NET Core, this could trigger JSON content negotiation, returning `"OK"` (with quotes) instead of plain text `OK`. The documentation strictly demands **plain text OK** without any surrounding characters.

## 4. Business Rule Edge Cases

*   **Installment Mappings:** As noted, asking for installments effectively disables them on the PayTR form due to the `no_installment` flaw. Asking for `1` installment sends an invalid `max_installment` value.
*   **Hash Calculation Strategy:** 
    *   **STEP 1 (Token):** The hashing logic is sound because `PayTrHashService` automatically appends `merchant_salt` internally, compensating for its omission in `PayTrClient`.
    *   **STEP 2 (Callback):** Uses constant-time comparison `CryptographicOperations.FixedTimeEquals()`, which properly prevents timing attacks.
*   **Idempotency & Duplicate Webhooks:** Handled correctly. If an order is already marked as `CompletedWithSuccess` or `CompletedWithFail`, subsequent webhook notifications are logged but do not overwrite the final status, and return `OK` to stop PayTR from retrying.

## 5. Error Code Coverage

The system parses `failed_reason_code` safely using `int.TryParse` with a fallback to `-1`. It looks up the fail reason via `PayTrFailReasonService`, which appears sufficient as long as that service contains the standard PayTR codes (`0, 1, 2, 3, 6, 8, 9, 10, 11, 99`).

## 6. Findings Summary Table

| Finding | Severity | Description |
| :--- | :--- | :--- |
| **Inverted `no_installment` logic** | **Critical** | Prevents installments from appearing when users request them, and displays them when they shouldn't. |
| **Invalid `max_installment` value** | **High** | Sending `"1"` violates the allowed enumeration (`0, 2-12`) and may cause PayTR to reject the token request. |
| **Missing `payment_amount` in Callback** | **Medium** | The application does not capture the original `payment_amount` from the webhook, only `total_amount`. |
| **Response Format Risk** | **Medium** | `Results.Ok("OK")` may return `"OK"` instead of `OK`, causing PayTR to flag the callback as failed and infinitely retry. |
| **Hardcoded `timeout_limit`** | **Low** | Hardcoded to 30 minutes; should be configurable per request or globally. |
| **Basket item price serialization** | **Low** | Should explicitly convert the `decimal` price to a string with two decimal places (e.g., `"18.00"`) before serialization. |
