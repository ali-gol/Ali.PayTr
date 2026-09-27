# PayTR Architectural Review

## 1. Architectural Assessment
The architecture cleanly follows the Dependency Inversion Principle, separating the package into four distinct layers:
- **`Ali.PayTr.Abstractions`**: Core models, enums, and interfaces.
- **`Ali.PayTr.Core`**: Business logic, API communication (`PayTrClient`), hash calculation, and webhook processing (`PayTrNotificationProcessor`).
- **`Ali.PayTr.AspNetCore`**: Minimal APIs integration for seamless webhook registration (`MapPayTrPaymentEndpoints`).
- **`Ali.PayTr.EFCore`**: Persistence layer for order state and notification history logging.

This structure allows consumers to opt-in only to the layers they need (e.g., opting out of EF Core if they use Dapper or MongoDB). 

## 2. Developer Experience (DX) & Abstraction Design
The package abstracts the complexity of the PayTR API effectively:
- Form-urlencoded payloads, Base64 JSON structures (e.g., `user_basket`), and hash combinations are encapsulated within `PayTrClient`.
- The `decimal` to `long/string` conversion (multiplying by 100 to avoid kuruş discrepancies) is safely handled internally.
- The webhook endpoint (`/notification`) handles parsing the form data, processing it, and automatically returning the required plain-text `"OK"` string to PayTR.

**Area for Improvement**: The `debug_on` parameter is hardcoded to `"1"` in `PayTrClient.cs`. This should be mapped to the `PayTrOptions.TestMode` flag to avoid leaking error details in production environments.

## 3. Security Architecture
- **Hash Verification**: The package uses `CryptographicOperations.FixedTimeEquals` for the webhook hash verification. This is a secure approach that mitigates timing attacks.
- **Payload Integrity**: The hash concatenation order strictly adheres to the official `STEP 2.md` documentation.
- **IP Address Tracking**: The system falls back to `127.0.0.1` if `ClientIp` is not provided. Since `PayTrClientIpAccessor` is registered, this is generally safe, but production environments relying on reverse proxies must configure forwarded headers correctly.

## 4. Resilience & Reliability
- **Webhook Idempotency**: `PayTrNotificationProcessor` successfully guarantees idempotency by checking `OrderStatus.CompletedWithSuccess` or `OrderStatus.CompletedWithFail` before modifying the database, ensuring duplicate PayTR callbacks do not result in double-processing.
- **API Communication Resiliency**: The `PayTrClient` currently uses a naked `HttpClient.PostAsync`. It lacks built-in transient fault handling (retries, timeouts, or circuit breakers). If the PayTR API drops the connection during a token request, the package will throw an unhandled `HttpRequestException`.

## 5. Error Handling Strategy
- **Result Pattern**: The package utilizes a Result pattern (`PayTrCreatePaymentResponse.IsSuccess`), avoiding expensive exceptions for standard API rejections.
- **Exception Leaks**: The `HttpClient` call in `PayTrClient.cs` should catch network-level exceptions and return a unified error response rather than crashing the caller's stack.
- **Error Mapping**: PayTR specific error codes are cleanly mapped into `PayTrFailReasonService`, abstracting magic numbers into readable `FailedReasonMsg` structures.

## 6. Package Integrity & Compatibility
- **Dependency Minimization**: The core logic relies primarily on BCL and standard Microsoft Extensions (Logging, Options, Http). 

## 7. Proposed Design Decisions Summary

1. **Decision**: Integrate Polly for `HttpClient` resilience.
   - **Rationale**: The `get-token` API call is synchronous relative to the checkout flow. Network blips shouldn't fail the checkout immediately.
   - **Impact**: Adds transient fault tolerance.
   - **Priority**: P1

2. **Decision**: Catch `HttpRequestException` in `PayTrClient`.
   - **Rationale**: The client currently leaks network exceptions instead of returning a failed `PayTrCreatePaymentResponse`.
   - **Impact**: Better DX via consistent Result pattern.
   - **Priority**: P1

3. **Decision**: Map `debug_on` dynamically.
   - **Rationale**: Hardcoding `debug_on = "1"` exposes internal error messages on the PayTR checkout page to end users. It should be `1` only when `TestMode` is enabled.
   - **Impact**: Prevents information disclosure in production.
   - **Priority**: P0
