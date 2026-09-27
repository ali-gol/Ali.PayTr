# 1. Architectural Assessment
The overall architecture of the PayTR Direct API implementation is well-structured. It successfully separates concerns via an `Abstractions` layer (interfaces and models) and a `Core` layer (services, API clients, and HTTP communication). The use of the Result pattern (`PayTrDirectPaymentResult`, `PayTrNotificationVerifyResult`) instead of throwing domain exceptions for HTTP-level errors is excellent for a robust SDK. The BA Report confirms that the implementation fully aligns with PayTR's business rules and tokenization specifications.

# 2. Developer Experience (DX) & Abstraction Design
- **API Quirks Isolation**: The `PayTrDirectPaymentRequest` and `PayTrDirectClient` successfully encapsulate PayTR's specific formatting requirements (e.g., translating `bool` to `"1"`/`"0"`, and `decimal` to `"0.00"` invariant strings). The base64 JSON serialization of `user_basket` is also hidden from the caller.
- **Multi-Tenancy Limitation**: Currently, `PayTrOptions` is registered via `IOptions<PayTrOptions>`, binding exactly one Merchant ID, Key, and Salt per application instance. This blocks multi-tenant B2B platforms where each tenant brings their own PayTR credentials.
- **Recommendations**: Introduce an `IPayTrCredentialsProvider` to allow dynamic retrieval of merchant credentials at runtime, falling back to `IOptions` if none is provided.

# 3. Security Architecture
- **Hash Validation**: The `PayTrNotificationProcessor` correctly employs `CryptographicOperations.FixedTimeEquals` to prevent timing attacks when verifying the webhook hash. 
- **PII/PCI Logging**: `PayTrDirectClient.cs` actively masks card numbers (`MaskCardNumber(request.CardNumber)`) before logging. However, it dumps the raw `ResponseString` at the `Trace` level. This is generally safe since PayTR responses do not echo full PANs, but log scrubbing should be strictly enforced.

# 4. Resilience & Reliability
- **HTTP Policies (Polly)**: The `IHttpClientBuilder` registrations in `ServiceCollectionExtensions.cs` (e.g., `AddPayTrDirectApi`) do not attach any transient error handling policies. If the PayTR API drops a connection momentarily, the client will fail immediately. 
- **Webhook Idempotency**: The `PayTrNotificationProcessor` handles duplicate webhook deliveries perfectly. It leverages database constraints/locks (via catching `PayTrConcurrencyException`) and a `wasFinalized` state check to ensure the client’s event handlers are only triggered once, while still returning `OK` to PayTR.

# 5. Error Handling Strategy
- The current SDK correctly uses a dedicated `PayTrFailReasonService` to map PayTR's numeric `failed_reason_code` back to human-readable developer errors. The unified event dispatcher (`IPayTrOrderEventDispatcher`) cleanly segregates System Errors from Payment Validation failures.

# 6. Package Integrity & Compatibility
- The library successfully avoids taking on heavy 3rd-party dependencies (like `Newtonsoft.Json`), preferring native `System.Text.Json` and built-in HTTP clients. This minimizes dependency conflicts for consumers.

# 7. Proposed Design Decisions Summary
1. **Decision**: Implement HTTP Resilience via Polly.
   **Rationale**: External APIs exhibit transient network failures. Implementing automatic retries (with exponential backoff) for `5xx` or timeout errors prevents failed checkout flows on the merchant side.
   **Impact**: Enhances SDK reliability.
   **Priority**: P1

2. **Decision**: Abstract Credentials Provisioning (`IPayTrCredentialsProvider`).
   **Rationale**: The current `IOptions` approach restricts the SDK to a single-tenant model.
   **Impact**: Broadens package applicability for SaaS platforms.
   **Priority**: P2
