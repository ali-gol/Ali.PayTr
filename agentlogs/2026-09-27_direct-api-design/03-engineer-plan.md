# PayTR Engineer Plan

## 1. Code-Level Audit Findings

### Finding 1: Amount Formatting Discrepancy
- **File**: `src/Ali.PayTr.Core/Clients/PayTrClient.cs`
- **Line(s)**: 30-33 (`ConvertAmountToString`)
- **Issue**: The current logic converts decimal amounts to kuruş (multiplying by 100). Reusing this for the Direct API will result in a 100x overcharge, as Direct API strictly expects standard decimal formatting (e.g., `100.99`). Furthermore, using `.ToString()` without `CultureInfo.InvariantCulture` on decimals is dangerous in locales where the decimal separator is a comma.
- **Severity**: Critical
- **Category**: Culture/Locale Bug & Type Conversion Error

### Finding 2: Hash Abstraction Leak
- **File**: `src/Ali.PayTr.Core/Clients/PayTrClient.cs`
- **Line(s)**: 70-82
- **Issue**: The specific string concatenation rules for HMAC-SHA256 hash generation are hardcoded inside the HTTP client. This violates the API Contract for Direct API, which requires a completely different concatenation order and omits basket logic.
- **Severity**: High
- **Category**: API Contract Violation

### Finding 3: Missing Required Validations
- **File**: `src/Ali.PayTr.Core/Clients/PayTrClient.cs`
- **Line(s)**: 44-55
- **Issue**: Despite `CustomerEmail` and `CustomerPhone` being marked as `required` in `PayTrCreatePaymentRequest.cs`, there is no runtime validation for empty strings before executing the API request. An empty string will silently distort the token hash concatenation, leading to unauthorized errors from PayTR.
- **Severity**: Medium
- **Category**: Missing Validation

### Finding 4: Inefficient Logging Allocations
- **File**: `src/Ali.PayTr.Core/Clients/PayTrClient.cs`
- **Line(s)**: 113, 132
- **Issue**: Using string interpolation (`$"{...}"`) directly within `_logger.LogTrace` allocates strings unconditionally, even when tracing is disabled.
- **Severity**: Low
- **Category**: Memory/Performance

## 2. Architectural Objections (Pragmatism Check)

1. **Decision: Create isolated `IPayTrDirectClient` and `PayTrDirectPaymentRequest`.**
   - *Endorsement:* Fully endorsed. Segregating the models ensures backward compatibility for existing IFrame integrations and strictly adheres to the Single Responsibility Principle.

2. **Decision: Encapsulate Hash String Generation in `IPayTrHashService`.**
   - *Endorsement:* Endorsed. Pushing concatenation rules into the domain service eliminates the leak in the HTTP client.

3. **Decision: Implement Payload Masking in Logging.**
   - *Endorsement:* Endorsed. Masking raw CC numbers and CVVs is non-negotiable for PCI-DSS compliance.

4. **Decision: Introduce concurrency control (Locking) in `PayTrNotificationProcessor` using Redis/SemaphoreSlim.**
   - *Objection:* Pushing distributed locking (e.g., Redis) into a core library is extreme over-engineering and forces a huge dependency on consumers. `SemaphoreSlim` is in-memory only and fails in web farms. 
   - *Leaner Alternative:* Let the repository handle native optimistic concurrency. Using EF Core's `RowVersion` or `ConcurrencyToken` will throw a `DbUpdateConcurrencyException` on concurrent identical webhooks, which we can catch and safely ignore (treating it as already processed) with zero added dependencies.

5. **Decision: Implement HTTP Resilience and Circuit Breakers (Polly).**
   - *Objection:* Highly risky and over-engineered. The Direct API `POST` endpoint directly triggers transactions without a documented idempotency key in the request payload. Retrying a timed-out POST may result in double-charging the customer.
   - *Leaner Alternative:* Use standard `HttpClient.Timeout` and fail fast on `TaskCanceledException`. Let the consuming application's business logic dictate retries based on database state or webhook confirmation.

## 3. Implementation Change Plan

1. **Direct API Models**
   - **File:** `src/Ali.PayTr.Abstractions/Models/PayTrDirectPaymentRequest.cs` (New)
   - **Class:** `PayTrDirectPaymentRequest`
   - **What changes:** Create new DTO containing Direct API-specific fields (`CardNumber`, `Cvv`, `ExpiryMonth`, `ExpiryYear`, `CardType`, `Non3D`, `SyncMode`).
   - **Why:** Required for the Direct API endpoint (BA finding) without leaking properties into IFrame users.
   - **Priority:** P0
   - **Complexity:** Simple

2. **Hash Service Refactoring**
   - **File:** `src/Ali.PayTr.Core/Services/PayTrHashService.cs`
   - **Class:** `PayTrHashService`
   - **What changes:** Expose `CreateIFrameToken` and `CreateDirectApiToken`. Ensure `CultureInfo.InvariantCulture` is applied rigorously during decimal-to-string conversions.
   - **Why:** Replaces the hardcoded client hash concatenation and handles the new Direct API hashing rules.
   - **Priority:** P0
   - **Complexity:** Moderate

3. **Direct Client Implementation**
   - **File:** `src/Ali.PayTr.Core/Clients/PayTrDirectClient.cs` (New)
   - **Class:** `PayTrDirectClient`
   - **What changes:** Implement `POST https://www.paytr.com/odeme`. Handle `sync_mode=1` JSON responses accurately. Implement strict payload validation. Map language to `client_lang` instead of `lang`.
   - **Why:** Directly addresses BA and Architect findings for integrating the new flow.
   - **Priority:** P0
   - **Complexity:** Complex

4. **Security & Structured Logging**
   - **File:** `src/Ali.PayTr.Core/Clients/PayTrDirectClient.cs`
   - **Class:** `PayTrDirectClient`
   - **What changes:** Implement payload masking before sending request strings to `ILogger`. Switch to structured logging templates.
   - **Why:** Fixes memory allocations and prevents logging sensitive PCI data.
   - **Priority:** P1
   - **Complexity:** Simple

5. **Webhook Optimistic Concurrency**
   - **File:** `src/Ali.PayTr.Core/Services/PayTrNotificationProcessor.cs`
   - **Class:** `PayTrNotificationProcessor`
   - **What changes:** Catch and handle `DbUpdateConcurrencyException` when updating order status. If caught, treat the notification as already successfully handled.
   - **Why:** Safely prevents race conditions without adding external locking dependencies.
   - **Priority:** P2
   - **Complexity:** Moderate

## 4. Unit Test Plan

- **Test Name:** `CreateDirectApiToken_ConstructsCorrectHash_WithExactOrder`
  - *What it verifies:* Validates that `PayTrHashService` concatenates the Direct API parameters in the exact mandated sequence.
  - *Summary:* Arrange dummy Direct Request. Act: generate token. Assert: Compare with a known HMAC-SHA256 signature.

- **Test Name:** `CreatePaymentAsync_AmountFormat_UsesInvariantCulture`
  - *What it verifies:* Validates that decimal values are sent with dots (e.g., `100.99`) regardless of thread culture.
  - *Summary:* Arrange thread culture to `tr-TR`. Act: invoke client with a decimal amount. Assert: Intercept the HTTP message to verify the payload string is formatted correctly.

- **Test Name:** `CreatePaymentAsync_MasksSensitiveData_InLogs`
  - *What it verifies:* `CardNumber` and `Cvv` are heavily masked (e.g., `4111********1111`) before being passed to `ILogger`.
  - *Summary:* Arrange mock `ILogger`. Act: execute Direct API call. Assert: Log intercepts contain no plain-text card data.

- **Test Name:** `CreatePaymentAsync_WithSyncMode1_ParsesJsonResponseCorrectly`
  - *What it verifies:* The client accurately maps `success`, `failed`, or `wait_callback` statuses from the PayTR JSON response.
  - *Summary:* Arrange mocked HttpMessageHandler returning JSON payloads. Act: call method. Assert: Result states and messages correspond exactly to the JSON.
