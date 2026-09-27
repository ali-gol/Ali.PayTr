# PayTR Senior Software Architect Review

## 1. Architectural Assessment
- **Current Architecture Status**: The current architecture successfully segregates abstractions (`Models`, `Interfaces`) from core implementations (`Clients`, `Services`), providing a solid and testable foundation. Dependency injection patterns and the options pattern are well-utilized.
- **What Doesn't Work**: The existing `PayTrClient` tightly couples the package to the IFrame API flow. It assumes `odeme/api/get-token` endpoints, applies kuruş conversions (`10099`), and returns an HTML redirect URL. Reusing this for the Direct API would violate the Single Responsibility Principle (SRP) and the Interface Segregation Principle (ISP). 
- **Hash Abstraction Leak**: Currently, `IPayTrHashService` only computes HMAC. The client is responsible for building the hash string. This leaks PayTR's arbitrary concatenation rules into the HTTP Client, weakening cohesion and creating duplication risks when introducing Direct API hash rules.

## 2. Developer Experience (DX) & Abstraction Design
- **Segregation of Clients**: To completely isolate IFrame complexity from Direct API complexity, introduce a dedicated `IPayTrDirectClient` (while keeping `IPayTrClient` exclusively for the IFrame flow). 
- **Model Segregation**: Create `PayTrDirectPaymentRequest` and `PayTrDirectPaymentResponse`. This isolates Direct API specific fields (e.g., `card_number`, `sync_mode`, `non_3d`) from standard IFrame users, ensuring clean API surfaces.
- **Amount Standardization**: Package consumers should ALWAYS deal with standard decimal formats (e.g., `100.99`). The underlying client/abstraction layer will handle formatting based on the target API. 
- **Fluent Builder**: Introduce a Fluent Builder pattern (e.g., `PayTrDirectPaymentRequestBuilder`) to simplify request construction for the Direct API. This allows field validation (especially for sensitive or complex combinations) before the HTTP call.

## 3. Security Architecture
- **Hash Generation Centralization**: Refactor `IPayTrHashService` to expose domain-specific methods such as `CreateIFrameToken(request, options)` and `CreateDirectApiToken(request, options)`. The hash service must completely encapsulate the exact string concatenation order and rules defined by PayTR.
- **Timing Attacks Defense**: The `PayTrNotificationProcessor` correctly uses `CryptographicOperations.FixedTimeEquals` for hash comparison. This is an excellent implementation and must be preserved.
- **Sensitive Data Masking**: Credit Card information (`card_number`, `cvv`, `expiry`) must be aggressively zeroed out or masked in logging sinks. The `ILogger` calls in the HTTP Client must sanitize these payload fields before writing.

## 4. Resilience & Reliability
- **HTTP Resilience (Polly)**: Integrate `Microsoft.Extensions.Http.Polly`. Because the Direct API endpoint (`odeme`) immediately triggers the payment (without an explicitly documented idempotency key in the request), retry policies must be applied extremely cautiously (e.g., only on DNS failures before the payload is sent or on safe 5xx errors if guaranteed not to double-charge).
- **Webhook Idempotency Race Condition**: The `PayTrNotificationProcessor` currently checks `oldStatus` for idempotency (`wasFinalized`). However, under high concurrency (e.g., PayTR firing duplicate Webhooks simultaneously), a race condition exists. We must introduce a distributed lock mechanism (like a `SemaphoreSlim` or a database/Redis lock on `MerchantOid`) to guarantee absolute idempotency.
- **Circuit Breaker**: Implement a circuit breaker to fail fast and degrade gracefully if PayTR APIs experience downtime.

## 5. Error Handling Strategy
- **Result Pattern Continuation**: The current use of the Result pattern (`IsSuccess`, `Message`, `CorrelationId`) in `PayTrCreatePaymentResponse` is effective. Continue this pattern for `PayTrDirectPaymentResponse`.
- **Sync Mode Support (`sync_mode=1`)**: When operating in sync mode, PayTR will return JSON directly. `IPayTrDirectClient` must accurately parse these `success`, `failed`, or `wait_callback` statuses. Map any PayTR failure reasons to domain error models using the existing `PayTrFailReasonService`.

## 6. Package Integrity & Compatibility
- **Backward Compatibility**: It is crucial that `IPayTrClient` and `PayTrCreatePaymentRequest` remain fully backward compatible. Do not introduce breaking changes to the IFrame flow. All new code must be additive.
- **Target Framework**: Maintain current standard multi-targeting compatibility (`netstandard2.0`, etc.) to ensure a broad reach for the NuGet package.
- **Minimal Dependencies**: Use `Polly` for resilience but avoid bloating the package with unnecessary external dependencies.

## 7. Proposed Design Decisions Summary

1. **Decision**: Create isolated `IPayTrDirectClient` and `PayTrDirectPaymentRequest`.
   - **Rationale**: Prevents SRP and ISP violations. Keeps existing IFrame API backward compatible.
   - **Impact**: Clean separation of concerns; additive only change.
   - **Priority**: P0

2. **Decision**: Encapsulate Hash String Generation in `IPayTrHashService`.
   - **Rationale**: Prevents hash rules leaking across clients; standardizes rule testing.
   - **Impact**: Refactoring of `PayTrClient` token generation logic.
   - **Priority**: P0

3. **Decision**: Implement Payload Masking in Logging.
   - **Rationale**: Credit card details and CVV must never be logged in raw format.
   - **Impact**: Security hardening.
   - **Priority**: P1

4. **Decision**: Introduce concurrency control (Locking) in `PayTrNotificationProcessor`.
   - **Rationale**: Prevents race conditions during simultaneous identical webhook callbacks from PayTR.
   - **Impact**: Enhances idempotency and prevents double processing.
   - **Priority**: P1

5. **Decision**: Implement HTTP Resilience and Circuit Breakers (Polly).
   - **Rationale**: Protects both the package consumer and PayTR APIs during network instability. 
   - **Impact**: Adds minor dependency (`Microsoft.Extensions.Http.Polly`).
   - **Priority**: P2
