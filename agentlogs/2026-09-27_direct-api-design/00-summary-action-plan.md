# PayTR Integration Summary & Action Plan

## Pipeline Overview
The multi-agent pipeline analyzed the PayTR NuGet package for integrating the "Direct API" alongside the existing IFrame API. The Business Analyst identified critical gaps in payload formatting (kuruş vs. decimal) and missing API parameters. The Architect proposed cleanly segregating the client implementations (`IPayTrDirectClient`), centralizing the hash logic, and hardening security. Finally, the Engineer pruned the over-engineered distributed locking and HTTP retries in favor of native EF Core optimistic concurrency and standard timeouts, yielding a highly pragmatic, zero-dependency implementation plan.

## Critical Issues (P0) — Fix Immediately

1. **Amount Formatting Discrepancy**
   - **Issue:** `PayTrClient.cs` currently converts decimals to kuruş (`10099`). Direct API requires standard decimals (`100.99`). Reusing the existing formatter will trigger 100x overcharges.
   - **File:** `src/Ali.PayTr.Core/Clients/PayTrClient.cs`
   - **Action Required:** Ensure Direct API implementation explicitly uses `CultureInfo.InvariantCulture` for standard decimal representations.

2. **Hash Generation Leak & Rule Mismatch**
   - **Issue:** `PayTrClient` hardcodes the IFrame hash concatenation logic. Direct API requires a completely different formula.
   - **File:** `src/Ali.PayTr.Core/Clients/PayTrClient.cs` & `src/Ali.PayTr.Core/Services/PayTrHashService.cs`
   - **Action Required:** Move concatenation logic into `PayTrHashService` and expose two separate domain methods: `CreateIFrameToken` and `CreateDirectApiToken`.

3. **Missing Direct API Endpoint & Models**
   - **Issue:** The Direct API posts to `https://www.paytr.com/odeme` with entirely new credit card fields and sync flags, which are completely absent in the current models.
   - **File:** `src/Ali.PayTr.Core/Clients/PayTrDirectClient.cs` (New)
   - **Action Required:** Create `IPayTrDirectClient` and `PayTrDirectPaymentRequest` to safely isolate these endpoints without violating SRP/ISP.

## High Priority (P1) — Fix Before Next Release

1. **Sensitive Data Logging Leak**
   - **Issue:** Raw Personal Account Numbers (PAN) and CVVs could be inadvertently captured in debug logs.
   - **File:** `src/Ali.PayTr.Core/Clients/PayTrDirectClient.cs`
   - **Action Required:** Implement payload masking to zero-out or asterisk sensitive PCI fields before logging.

2. **Missing Sync Mode (`sync_mode=1`) Handling**
   - **Issue:** Synchronous JSON responses are currently unhandled, breaking real-time transaction confirmations.
   - **File:** `src/Ali.PayTr.Core/Clients/PayTrDirectClient.cs`
   - **Action Required:** Implement robust JSON parsing for `success`, `failed`, or `wait_callback` statuses.

## Medium Priority (P2) — Plan for Near-Term

1. **Webhook Idempotency Race Condition**
   - **Issue:** Highly concurrent PayTR webhook deliveries could be processed multiple times.
   - **File:** `src/Ali.PayTr.Core/Services/PayTrNotificationProcessor.cs`
   - **Action Required:** Implement EF Core optimistic concurrency (`DbUpdateConcurrencyException` handling) to gracefully ignore duplicates instead of pulling in external Redis locks.

2. **Language Parameter Correction**
   - **Issue:** The IFrame API uses `lang`, while Direct API specifically expects `client_lang`.
   - **File:** `src/Ali.PayTr.Abstractions/Models/PayTrDirectPaymentRequest.cs`
   - **Action Required:** Ensure the new Direct API payload uses `client_lang`.

## Low Priority (P3) — Backlog

1. **String Interpolation in Logging**
   - **Issue:** Use of string interpolation in logger calls forces allocations.
   - **File:** `src/Ali.PayTr.Core/Clients/PayTrClient.cs`
   - **Action Required:** Refactor existing logs to use structured templating.

## Missing Features (Gap Summary)

| Feature | Status | Effort Estimate |
|---------|--------|-----------------|
| Direct API Token Hash Logic | ❌ Missing | Moderate |
| Credit Card Models & Masking | ❌ Missing | Simple |
| Sync Mode JSON Parsing | ❌ Missing | Moderate |
| `IPayTrDirectClient` Implementation | ❌ Missing | Complex |

## Recommended Implementation Order

1. **Model & Interface Segregation:** Create `PayTrDirectPaymentRequest` and `IPayTrDirectClient`.
2. **Hash Service Refactor:** Build `CreateIFrameToken` and `CreateDirectApiToken` within `PayTrHashService`.
3. **Direct Client Implementation:** Build `PayTrDirectClient` incorporating the POST endpoint, strict formatting (`CultureInfo.InvariantCulture`), `client_lang`, and sync mode parsing.
4. **Logging & Security:** Introduce strict PCI payload masking and correct logger allocations.
5. **Concurrency Fix:** Add EF Core optimistic concurrency handling to `PayTrNotificationProcessor`.

## Test Coverage Gaps

- **Hash Concatenation:** Strict unit tests for the exact Direct API hash sequence.
- **Culture Invariance:** Tests simulating non-US locales (e.g., `tr-TR`) to guarantee no comma decimal rendering.
- **Log Masking:** Unit tests enforcing that credit card numbers/CVVs never reach the `ILogger`.
- **JSON Deserialization:** Mock HTTP handler tests for validating `sync_mode=1` response models.
