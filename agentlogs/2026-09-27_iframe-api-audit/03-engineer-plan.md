# PayTR Engineering Audit & Implementation Plan

## 1. Code-Level Audit Findings

1. **File**: `src/Ali.PayTr.Core/Clients/PayTrClient.cs`
   - **Line(s)**: 32
   - **Issue**: `Math.Round(amount * 100)` defaults to `MidpointRounding.ToEven`. This can cause 1-kuruş discrepancies in certain payment amounts, leading to hash mismatch or incorrect charges.
   - **Severity**: High
   - **Category**: Type Conversion Error

2. **File**: `src/Ali.PayTr.Core/Clients/PayTrClient.cs`
   - **Line(s)**: 82
   - **Issue**: `debug_on` is hardcoded to `"1"`. This exposes sensitive error traces to the user interface in production.
   - **Severity**: High
   - **Category**: Security Vulnerability

3. **File**: `src/Ali.PayTr.Core/Services/PayTrNotificationProcessor.cs`
   - **Line(s)**: 45
   - **Issue**: `Guid.Parse(notification.MerchantOid)` throws an unhandled `FormatException` if a malformed `MerchantOid` is received from the webhook, crashing the request before it can log the failure.
   - **Severity**: Medium
   - **Category**: Missing Validation

4. **File**: `src/Ali.PayTr.Core/Clients/PayTrClient.cs`
   - **Line(s)**: 96
   - **Issue**: `HttpClient.PostAsync` can throw `HttpRequestException` or `TaskCanceledException` (on timeout). These are not caught, violating the `IsSuccess` Result pattern used by `PayTrCreatePaymentResponse`.
   - **Severity**: Medium
   - **Category**: API Contract Violation

## 2. Architectural Objections (Pragmatism Check)

- **Review on "Integrate Polly for HttpClient resilience"**:
  - **Objection**: I strongly push back on adding Polly for automatic retries on the `get-token` endpoint. 
  - **Why**: The `get-token` call happens synchronously when the user clicks "Checkout". Retrying a timed-out token request can lead to orphaned tokens on PayTR's side, and it prolongs the user's wait time unnecessarily. A simple failure with a prompt to "Try Again" is safer and leaner. 
  - **Alternative**: Rely on standard `HttpClient.Timeout` and wrap the call in a `try-catch` to return a graceful failure result.
- **Review on "Catch HttpRequestException in PayTrClient"**: Endorsed.
- **Review on "Map debug_on dynamically"**: Endorsed.

## 3. Implementation Change Plan

1. **File**: `src/Ali.PayTr.Core/Clients/PayTrClient.cs`
   - **Class/Method**: `PayTrClient.ConvertAmountToString`
   - **What changes**: Change to `return ((long)Math.Round(amount * 100, MidpointRounding.AwayFromZero)).ToString();` or simply cast `(long)(amount * 100m)`.
   - **Why**: Fixes MidpointRounding bug.
   - **Priority**: P1
   - **Estimated Complexity**: Simple

2. **File**: `src/Ali.PayTr.Core/Clients/PayTrClient.cs`
   - **Class/Method**: `PayTrClient.CreatePaymentAsync`
   - **What changes**: Update payload generation: `["debug_on"] = _options.TestMode ? "1" : "0"`. Wrap the HTTP call in `try/catch (HttpRequestException)` returning `IsSuccess = false`.
   - **Why**: Fixes Security Vulnerability & API Contract Violation.
   - **Priority**: P1
   - **Estimated Complexity**: Simple

3. **File**: `src/Ali.PayTr.Core/Services/PayTrNotificationProcessor.cs`
   - **Class/Method**: `PayTrNotificationProcessor.ProcessNotificationAsync`
   - **What changes**: Change `Guid.Parse` to `if (!Guid.TryParse(..., out var correlationId)) return false/log error;`.
   - **Why**: Prevents 500 errors on malicious or malformed webhook payloads.
   - **Priority**: P2
   - **Estimated Complexity**: Simple

## 4. Unit Test Plan

- **`PayTrClientTests`**:
  - `ConvertAmountToString_ShouldRoundAwayFromZero`: Verifies `34.565m` becomes `3457` instead of `3456`.
  - `CreatePaymentAsync_WhenNetworkFails_ReturnsFailedResult`: Mock HttpMessageHandler to throw `HttpRequestException`, assert `IsSuccess == false`.
  - `CreatePaymentAsync_WhenTestModeFalse_SetsDebugOnZero`: Verifies security fix.
- **`PayTrNotificationProcessorTests`**:
  - `ProcessNotificationAsync_MalformedOid_ReturnsFalse`: Sends `"invalid-guid"` and expects handled rejection.
  - `ProcessNotificationAsync_ValidDuplicate_IsIdempotent`: Sends the same valid notification twice, verifies DB is updated only once.
