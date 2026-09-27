# PayTR Business Analyst Report - IFrame API Audit

## 1. Scope & Methodology
- **Analyzed Scope**: IFrame API (`get-token` endpoint for token generation) and the Callback Notification (Webhook) integration.
- **Consulted Documentation**:
  - `docs/PayTR IFrame API/STEP 1/STEP 1.md`
  - `docs/PayTR IFrame API/STEP 2/STEP 2.md`
  - `docs/PayTR Error Codes.md`
- **Codebase Scope**: `src/Ali.PayTr.Abstractions` and `src/Ali.PayTr.Core`.

## 2. Endpoint & Feature Coverage Matrix

| Feature | Status | Notes |
|---------|--------|-------|
| IFrame Token Request (STEP 1) | ✅ Implemented | Token request payload correctly structured and sent to `odeme/api/get-token`. |
| Callback Notification (STEP 2) | ✅ Implemented | Webhook payload parsed and correctly processed. |
| Hash Generation & Validation | ✅ Implemented | Both Token generation hash and Webhook callback hash match documentation exactly. |
| Idempotency (Duplicate Callbacks) | ✅ Implemented | `PayTrNotificationProcessor` handles duplicate webhooks gracefully without duplicate order processing. |
| Error Code Handling | ✅ Implemented | Error codes strictly mapped according to the official documentation. |

## 3. Parameter Gap Details

### Token Request (`get-token`)
- **Implemented Parameters**: `merchant_id`, `user_ip`, `merchant_oid`, `email`, `payment_amount`, `paytr_token`, `user_basket`, `no_installment`, `max_installment`, `user_name`, `user_address`, `user_phone`, `merchant_ok_url`, `merchant_fail_url`, `timeout_limit`, `currency`, `test_mode`, `lang`.
- **Gaps / Issues**:
  - `debug_on`: Hardcoded to `"1"` in `PayTrClient.cs` (Line 82). While useful for testing, this will expose detailed error messages on the checkout page to end-users in production. It should be configurable or tied to `test_mode`.

### Callback Webhook
- **Implemented Parameters**: `merchant_oid`, `status`, `total_amount`, `hash`, `failed_reason_code`, `failed_reason_msg`, `test_mode`, `payment_type`, `currency`, `payment_amount`.
- **Gaps / Issues**: None found. Parameter mappings align with the API response payload.

## 4. Business Rule Edge Cases

- **Amount Handling**: `payment_amount` correctly multiplied by 100 via `(long)Math.Round(amount * 100)` avoiding kuruş discrepancies.
- **Installment Handling**: Configured correctly based on `InstallmentCount`.
- **Webhook Retry Behavior**: Idempotent. The API correctly responds after checking if the order status is already finalized.
- **Hash Validation**: Proper cryptographically safe timing check used (`CryptographicOperations.FixedTimeEquals`) in `PayTrNotificationProcessor.cs`.

## 5. Error Code Coverage
- The package implements exactly the error codes documented for the IFrame callback (0, 1, 2, 3, 6, 8, 9, 10, 11, 99) inside `PayTrFailReasonService.cs`.

## 6. Findings Summary Table

| Severity | Finding | Location |
|----------|---------|----------|
| High | `debug_on` is hardcoded to "1", which displays errors on the UI and could leak system state in production. | `PayTrClient.cs` Line 82 |
| Low | `ClientIp` falls back to `127.0.0.1` if not provided. | `PayTrClient.cs` Line 76 |
