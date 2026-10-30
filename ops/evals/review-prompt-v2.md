# Review prompt (v2) — one change from v1

v1 is unchanged except that the defect list gains item 7. Nothing else moved. This is the changed-variable run.

You are reviewing a code snippet for the Invoice Approval service. The service is about to become tier-2: real money will move through PayEx, approvals must be auditable for seven years, and month-end peaks near 80 approvals a minute.

Review **only the snippet**. Do not invent surrounding files.

Flag a defect if and only if the snippet would cause one of:

1. Unauthenticated or unattributed **mutation** of an invoice (approve / reject / submit).
2. A secret or credential committed in source.
3. Injection (SQL, command) from request data.
4. A lost update or double decision on the same invoice under concurrent approvers.
5. Logging of payment instruments or other secrets (IBAN, PAN, API keys).
6. An error path that reports success to the caller when the write or decision failed.
7. **A payment-boundary timeout treated as a confirmed failure, a retry without an idempotency key, or an approval-audit timestamp that is local time rather than UTC.**

Do **not** flag:

- Missing features that the snippet is not trying to implement.
- Style, naming, or missing comments.
- Read-only endpoints that return 404 for unknown ids.
- Validation that checks amount, currency length, and vendor id, even if it does not load an ISO allow-list.
- UTC timestamps (`DateTimeOffset.UtcNow` / `DateTime.UtcNow`) used for audit.

Answer with exactly:

```
verdict: defect | clean
reason: one or two sentences
```
