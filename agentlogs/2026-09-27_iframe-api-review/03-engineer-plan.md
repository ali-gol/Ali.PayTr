# PayTR Senior Software Engineer Plan

## 1. Code-Level Audit Findings

*   **File:** `src/Ali.PayTr.Core/Clients/PayTrClient.cs`
    *   **Line:** 79-80
    *   **Issue:** Inverted logic for `no_installment` and invalid value `1` sent for `max_installment`.
    *   **Severity:** Critical
    *   **Category:** API Contract Violation
*   **File:** `src/Ali.PayTr.Core/Clients/PayTrClient.cs`
    *   **Line:** 51
    *   **Issue:** Serializing decimal `Price` directly to JSON basket string creates format mismatches (e.g. `18.5` instead of `"18.50"`).
    *   **Severity:** Medium
    *   **Category:** Culture/Locale Bug
*   **File:** `src/Ali.PayTr.AspNetCore/Endpoints/PayTrNotificationEndpoint.cs`
    *   **Line:** 50
    *   **Issue:** `Results.Ok("OK")` relies on default content-negotiation, returning `"OK"` (JSON) rather than the strict plain text `OK` PayTR demands.
    *   **Severity:** High
    *   **Category:** API Contract Violation
*   **File:** `src/Ali.PayTr.AspNetCore/Endpoints/PayTrNotificationEndpoint.cs`
    *   **Line:** 31-41
    *   **Issue:** Fails to read the `payment_amount` form field from the webhook payload.
    *   **Severity:** Medium
    *   **Category:** Missing Validation / Missing Field
*   **File:** `src/Ali.PayTr.Core/Services/PayTrNotificationProcessor.cs`
    *   **Line:** 105-119
    *   **Issue:** `wasFinalized` check and subsequent `UpdateOrderAsync` are not atomic. A race condition exists for concurrent duplicate webhooks.
    *   **Severity:** Low
    *   **Category:** Concurrency / Memory/Performance

## 2. Architectural Objections (Pragmatism Check)

*   **Custom JSON Converter / Formatter Service:** *Endorse with modification.* We don't need a heavy custom service. A simple `.Select(x => new object[] { x.Name, x.Price.ToString("0.00", CultureInfo.InvariantCulture), x.Quantity })` directly in `PayTrClient` is entirely sufficient and prevents over-engineering.
*   **Distributed Lock for Idempotency:** *Object.* Forcing a distributed locking mechanism (Redis, etc.) onto consumers of a NuGet package is massive over-engineering and violates the library's lightweight scope.
    *   *Alternative:* Rely on EF Core's built-in Optimistic Concurrency (RowVersion) on the `PayTrOrder` entity, or accept the minor race condition since both threads will simply attempt to set the status to the exact same terminal state.
*   **Polly HTTP Resilience:** *Object (Partially).* Adding retries for the `get-token` endpoint (IFrame API) is dangerous. This is an interactive user flow. If PayTR is down, we want to fail fast so the user isn't stuck staring at a spinner for 10+ seconds while Polly retries. Retries are fine for Query/Refund, but should be strictly limited or omitted for `get-token`.
*   **Refactor `IPayTrHashService`:** *Endorse.* The hidden concatenation of `merchant_salt` inside the service is a footgun. The client should pass the fully concatenated string.

## 3. Implementation Change Plan

| File | Component | Description of Change | Priority | Complexity |
| :--- | :--- | :--- | :--- | :--- |
| `PayTrClient.cs` | `CreatePaymentAsync` | Fix installment logic: if `InstallmentCount == 1` => `no_installment="1"`, `max_installment="0"`. If `>1` => `no_installment="0"`, `max_installment=InstallmentCount.ToString()`. | P0 | Simple |
| `PayTrNotificationEndpoint.cs` | `HandleAsync` | Change return statement to `Results.Text("OK", "text/plain")`. | P0 | Simple |
| `PayTrClient.cs` | `CreatePaymentAsync` | Format basket item price using `ToString("0.00", CultureInfo.InvariantCulture)` before JSON serialization. | P1 | Simple |
| `PayTrNotificationEndpoint.cs` | `HandleAsync` | Read `payment_amount` from form and assign to `PayTrNotificationRequest.PaymentAmount`. | P1 | Simple |
| `PayTrOptions.cs` & `PayTrCreatePaymentRequest.cs` | Properties | Add `TimeoutLimitMinutes` (default 30). | P2 | Simple |
| `PayTrHashService.cs` & `PayTrClient.cs` | Hash Logic | Move `merchant_salt` concatenation out of `PayTrHashService` and into the caller (`PayTrClient` and `PayTrNotificationProcessor`). | P2 | Moderate |

## 4. Unit Test Plan

1.  **Service: `PayTrClient`**
    *   *Test:* `CreatePaymentAsync_SingleInstallment_SendsCorrectInstallmentParams` - Verifies `no_installment=1` and `max_installment=0`.
    *   *Test:* `CreatePaymentAsync_MultipleInstallments_SendsCorrectInstallmentParams` - Verifies `no_installment=0` and `max_installment=N`.
    *   *Test:* `CreatePaymentAsync_BasketFormatting_UsesInvariantStringDecimals` - Intercepts HTTP request to verify basket JSON contains string decimals like `"18.50"`.
2.  **Service: `PayTrNotificationEndpoint`**
    *   *Test:* `HandleAsync_AlwaysReturnsPlainTextOk` - Asserts the HTTP response content type is strictly `text/plain` and content is exactly `OK`.
3.  **Service: `PayTrHashService`**
    *   *Test:* `CreateTokenHash_DoesNotImplicitlyAppendSalt` - Asserts that the hash calculation strictly hashes exactly the bytes provided, ensuring callers must handle salt concatenation.
