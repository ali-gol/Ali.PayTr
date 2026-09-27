# 1. Code-Level Audit Findings
- **File**: `src/Ali.PayTr.Abstractions/Models/PayTrDirectPaymentRequest.cs`
  - **Issue**: Lack of DataAnnotations (e.g., `[StringLength]`, `[RegularExpression]`) on fields like `CardNumber`, `ExpiryMonth`, `MerchantOid`. PayTR docs enforce strict lengths (e.g., `user_ip` max 39, `merchant_oid` max 64, `expiry_month` 1-12). Relying only on `required` keyword causes late failures at the PayTR API level instead of failing fast locally.
  - **Severity**: Medium
  - **Category**: Missing Validation

- **File**: `src/Ali.PayTr.Core/Services/PayTrNotificationProcessor.cs` (Line 62-63)
  - **Issue**: Array allocations on every webhook request for timing-safe string comparison (`System.Text.Encoding.UTF8.GetBytes`). While functional and secure, it allocates unnecessary memory.
  - **Severity**: Low
  - **Category**: Memory/Performance

- **File**: `src/Ali.PayTr.Core/Clients/PayTrDirectClient.cs` (Line 103)
  - **Issue**: `HttpClient.PostAsync` receives a `CancellationToken`, which is correct. However, if the client application cancels the token, it raises a `TaskCanceledException`, which is caught and returned as a failed `PayTrDirectPaymentResult`. This masks the cancellation intent from the caller.
  - **Severity**: Low
  - **Category**: Error Handling

# 2. Architectural Objections (Pragmatism Check)
- **Review of Polly Integration (P1)**: 
  - **Objection**: I strongly object to blindly applying Polly retry policies (e.g., `AddTransientHttpErrorPolicy`) to the `PayTrDirectClient`. 
  - **Rationale**: The `CreatePaymentAsync` method executes a `POST` request to `https://www.paytr.com/odeme`. Retrying a POST request on a network timeout is highly dangerous in payment systems unless the endpoint is explicitly idempotent. If the request reached PayTR but the response timed out, a retry could authorize the card twice.
  - **Alternative**: Implement retries *only* for HTTP 503/502/504 errors where we can guarantee the payload wasn't processed, OR verify PayTR's idempotency guarantees around `merchant_oid` before enabling retries. For now, fail fast and let the user retry.

- **Review of IPayTrCredentialsProvider (P2)**:
  - **Endorsement**: I fully support this. The current `IOptions` singleton pattern is a blocker for multi-tenant SaaS applications.

# 3. Implementation Change Plan
1. **File**: `src/Ali.PayTr.Abstractions/Models/PayTrDirectPaymentRequest.cs`
   - **What**: Add DataAnnotations (`[MaxLength(64)]`, `[Required]`, `[RegularExpression]`) to enforce PayTR constraints before hashing.
   - **Why**: Fail-fast input validation.
   - **Priority**: P2
   - **Complexity**: Simple

2. **File**: `src/Ali.PayTr.Core/DependencyInjection/ServiceCollectionExtensions.cs`
   - **What**: Introduce `IPayTrCredentialsProvider` interface and a default `OptionsPayTrCredentialsProvider` implementation.
   - **Why**: Multi-tenancy support (Architect Decision #2).
   - **Priority**: P2
   - **Complexity**: Moderate

# 4. Unit Test Plan
1. **PayTrDirectClient Tests**:
   - *Test*: `CreatePayment_ShouldFormatAmountsCorrectly_InvariantCulture`
   - *Verify*: Passing decimal `100.99` with a Turkish culture thread does not serialize to `100,99`.
2. **PayTrHashService Tests**:
   - *Test*: `CreateDirectApiToken_ShouldMatchExpectedHash`
   - *Verify*: Hash string generation precisely matches the expected Base64 token given static inputs.
3. **PayTrNotificationProcessor Tests**:
   - *Test*: `ProcessNotificationAsync_ShouldReturnSuccess_WhenHashIsValid`
   - *Verify*: Happy path callback sets order status to `CompletedWithSuccess`.
   - *Test*: `ProcessNotificationAsync_ShouldFail_WhenHashIsTampered`
   - *Verify*: Mismatched hashes result in `SYSTEM_HASH_MISMATCH` failure dispatch.
