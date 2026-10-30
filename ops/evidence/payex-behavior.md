# PayEx behavior

Contract facts the team was given. There is no client in this repository yet.

- `POST /submissions` with invoice id, amount, currency, vendor payee, and an idempotency key.
- Success (200/201) returns `{ "reference": "PX-..." }`.
- The gateway times out at 30 seconds. The client we have been told to use surfaces that as a timeout exception.
- **A timeout does not mean the submission failed.** PayEx may have persisted the payment and failed to return the reference.
- Duplicate submissions without an idempotency key create two payments.
- Duplicate submissions with the same idempotency key return the original reference.
- There is no documented "get by invoice id" read API we can treat as strongly consistent within the timeout window. A nightly CSV of accepted references is available the next business morning.

This is why "retry on timeout" without a key is a double-pay, and why "optimistic submit and reconcile tomorrow" leaves a day of uncertainty at month-end.
