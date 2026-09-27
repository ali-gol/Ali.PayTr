# PayTR Senior Software Architect Review

## 1. Architectural Assessment

The current architecture follows a clean separation of concerns (`Abstractions` -> `Core` -> `AspNetCore`), which successfully isolates the PayTR integration details from consuming applications. The use of the Result pattern for API responses (`PayTrCreatePaymentResponse`) and event dispatchers for webhooks (`IPayTrOrderEventHandler`) are solid design choices.

However, the architecture suffers from overloaded responsibilities in `PayTrClient`. Specifically, it mixes HTTP communication, complex domain rule mapping (installments), and PayTR-specific data serialization (JSON basket, string conversion) into a single method. This makes the client brittle and hard to test.

## 2. Developer Experience (DX) & Abstraction Design

Addressing the BA findings regarding parameters and formatting requires better abstractions:

*   **Basket Item Serialization:** Directly using `JsonSerializer` on models containing `decimal` properties yields numeric JSON formats, whereas PayTR expects string-based formatting (e.g., `"18.00"`). 
    *   *Design:* Introduce a custom `JsonConverter<PayTrBasketItem>` or a specific `PayTrBasketFormatter` service that enforces `ToString("0.00", CultureInfo.InvariantCulture)` to guarantee strict compliance with PayTR's undocumented expectations.
*   **Installment Logic Mapping:** The inverted logic for `no_installment` and `max_installment` identified by the BA indicates that mapping domain intents (`InstallmentCount`) to PayTR's idiosyncratic fields is error-prone.
    *   *Design:* Move this logic out of `PayTrClient` and into a dedicated `PayTrParameterMapper` or extension methods. Introduce an explicit `InstallmentOptions` config or request model property that safely abstracts `0` and `1` behaviors for `no_installment` and bounds-checks the `max_installment` (e.g., ensuring `max_installment` is `0` when `InstallmentCount` is `1`).
*   **Callback Response Formatting:** Returning `Results.Ok("OK")` leaves the content type to the whims of ASP.NET Core formatters, which may wrap it in JSON.
    *   *Design:* The `PayTrNotificationEndpoint` must explicitly use `Results.Text("OK", "text/plain")` to guarantee a compliant webhook response.
*   **Timeout Configuration:** `timeout_limit` is currently hardcoded.
    *   *Design:* Add a `TimeoutLimitMinutes` property to `PayTrOptions` (global) and `PayTrCreatePaymentRequest` (per-request override) to enhance DX.

## 3. Security Architecture

*   **Webhook Hash Verification:** The existing `PayTrNotificationProcessor` correctly utilizes `CryptographicOperations.FixedTimeEquals()`, preventing timing attacks. This must be maintained.
*   **Hash Service Brittleness:** `PayTrHashService.CreateTokenHash` internally concatenates the `merchant_salt` to the provided string. This is a side-effect that is not obvious to the caller (`PayTrClient`).
    *   *Design:* Refactor `IPayTrHashService` to be more explicit. Instead of taking a pre-concatenated string, it should either take the raw payload object and handle the entire stringification/concatenation internally according to PayTR's specific order, OR the `merchant_salt` concatenation should happen explicitly in the caller before passing to a generic HMAC service.

## 4. Resilience & Reliability

*   **HTTP Resilience:** `ServiceCollectionExtensions` registers `HttpClient`s without any transient fault handling.
    *   *Design:* Integrate `Microsoft.Extensions.Http.Resilience` (or Polly) to add standard retries, timeouts, and circuit breakers for `IPayTrClient`, protecting the application from transient PayTR API outages.
*   **Webhook Idempotency Race Conditions:** The idempotency check (`wasFinalized`) in `PayTrNotificationProcessor` reads the order status and updates it. In high-concurrency scenarios where PayTR fires duplicate webhooks simultaneously, this creates a race condition that could lead to processing the notification twice.
    *   *Design:* Implement a distributed lock (e.g., using `merchant_oid` as the key) around the notification processing block, or rely on database-level row versions (optimistic concurrency) on the `PayTrOrder` entity to reject concurrent updates.

## 5. Error Handling Strategy

*   The current Result pattern (`PayTrCreatePaymentResponse`) is appropriate and prevents exception-driven control flow for expected API errors.
*   The `PayTrFailReasonService` effectively maps PayTR's magic numbers to meaningful descriptions. Ensure it fails gracefully if an unknown code is provided by PayTR.

## 6. Package Integrity & Compatibility

*   **Dependency Minimization:** Do not introduce heavy third-party JSON libraries (like Newtonsoft); stick to `System.Text.Json` to keep the package footprint small.
*   **API Surface:** The proposed changes to `PayTrCreatePaymentRequest` (adding `TimeoutLimitMinutes`) and options are backward compatible.

## 7. Proposed Design Decisions Summary

1.  **Decision:** Explicitly format webhook response as plain text.
    *   **Rationale:** Fixes the medium-severity BA finding where JSON content negotiation could break PayTR's callback loop.
    *   **Impact:** Modifies `PayTrNotificationEndpoint.cs`.
    *   **Priority:** P0 (Critical for webhook reliability).

2.  **Decision:** Correct the installment parameter mapping logic.
    *   **Rationale:** Fixes the critical BA finding where installment options are hidden or invalid (`max_installment="1"`).
    *   **Impact:** Modifies `PayTrClient.cs`. If `InstallmentCount == 1`, send `no_installment=1` and `max_installment=0`. If `>1`, send `no_installment=0` and `max_installment=InstallmentCount.ToString()`.
    *   **Priority:** P0 (Critical for checkout functionality).

3.  **Decision:** Format basket items securely using a custom formatter.
    *   **Rationale:** Prevents unpredictable decimal serialization in JSON payloads, adhering to PayTR's undocumented string-decimal requirement.
    *   **Impact:** Modifies basket JSON generation in `PayTrClient.cs`.
    *   **Priority:** P1.

4.  **Decision:** Include `payment_amount` in notification processing.
    *   **Rationale:** Addresses the medium BA finding. Capturing this allows the system to differentiate between the base price and the installment-inflated `total_amount`.
    *   **Impact:** Adds `PaymentAmount` to `PayTrNotificationRequest` and reads it in `PayTrNotificationEndpoint`.
    *   **Priority:** P1.

5.  **Decision:** Add Polly resilience to `AddPayTrPaymentsCore`.
    *   **Rationale:** Standardizes HTTP resilience against transient network failures.
    *   **Impact:** Modifies `ServiceCollectionExtensions.cs`.
    *   **Priority:** P2.
