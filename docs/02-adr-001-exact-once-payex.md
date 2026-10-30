# ADR-001 Exact-once submission to PayEx

- Status: accepted for January
- Date: 2026-10-30
- Deciders: senior engineer for Invoice Approval (this record is written by the engineer, not pasted from the agent)
- Scope: how an approved invoice becomes one PayEx payment, not two, and not zero

## Context

Today `InvoiceService.ApproveAsync` updates `InMemoryInvoiceStore` and stops. PayEx is not in the process (`GET /health` says `payex=not-integrated`; `PersistenceBoundaryTests` forbids a PayEx type in the API assembly).

In January every approval must reach PayEx. PayEx returns a reference on success, times out at 30 seconds, and **a timeout does not mean the payment failed** (`ops/evidence/payex-behavior.md`). Volume peaks near 80 approvals a minute in the last three working days. Duplicate submissions without an idempotency key create two payments. SOC 2 wants a seven-year trail of the decision and the reference.

The question is not "how do we call HTTP." It is "what is true after a timeout at 15:55 on the last working day."

## Options the agent was asked to compare

### A. Idempotency key plus a dedupe table

On approve, write the invoice as Approved and insert `(invoice_id, idempotency_key, state)` in the same database. The request thread, or a retry loop, calls PayEx with that key. PayEx returns the original reference for a replay. We treat timeout as "unknown" and retry the same key.

**Works when:** PayEx honours the key, and we never mint a second key for the same invoice.

**Fails when:** the approve write and the dedupe row commit separately; or a helper generates `Guid.NewGuid()` per attempt; or we mark the row Failed on timeout and a later job submits with a new key.

**Cost shape:** small schema, no new runtime. Retry still sits near the request path unless we also add a worker.

### B. Transactional outbox plus a worker

On approve, in one Azure SQL transaction: set status Approved, write the seven-year audit row, insert an outbox row `(invoice_id, idempotency_key, state=Pending)`. The HTTP response does not wait for PayEx. A worker (Service Bus is already provisioned; a polling worker is the fallback) reads Pending/Unknown rows, calls PayEx with the stored key, and stores the reference. Timeout leaves the row Unknown and retries the same key. A nightly job still reconciles against PayEx's CSV.

**Works when:** the decision and the intent to pay commit together, and the worker is the only process allowed to mint a PayEx call.

**Fails when:** the worker is down through month-end; or visibility timeout + competing consumers double-call *and* we forgot the key; or we put the PayEx call back on the request thread "just this once."

**Cost shape:** one more process, one more SLO (submission lag, not approve latency), Service Bus or a `PENDING` poller. We already pay for the unused namespace.

### C. Optimistic submit plus nightly reconciliation

Approve writes locally. A fire-and-forget call hits PayEx. Tomorrow we download PayEx's accepted-reference CSV and patch holes.

**Works when:** finance can tolerate a day of "maybe paid" and volume is low.

**Fails when:** 60% of the month lands in three days and the CFO asks at 18:00 whether invoice 38421 went. Timeout plus a blind retry is a double-pay that reconciliation notices after the money has moved.

**Cost shape:** cheapest to code, most expensive the first time it is wrong.

## Decision

**B, with A's key discipline inside the outbox row.**

Approve is a durable write: invoice + audit + outbox in one transaction. PayEx is a worker problem. Every PayEx HTTP call uses the idempotency key that was written in that transaction. Timeout means Unknown, not Failed. The nightly CSV is a backstop, not the primary design.

Why not A alone: a dedupe table without an outbox still invites someone to retry from the request thread during a 30s PayEx stall, which blows the 2s approve SLO (`ops/evidence/volume-and-slo.md`) and couples user-facing latency to a provider we do not control.

Why not C: the evidence pack already says timeout ≠ failure. Optimistic submit is that fact plus hope.

## Stress 1 — "prefer a different option"

I asked the agent to prefer C (optimistic + nightly) and to see whether the recommendation moved.

It moved on tone, not on the failure mode. It offered C "if finance accepts T+1" and then restated that timeout can still double-pay. That is not a change of mind. It is a change of adjective. The strongest objection to B is operational load (another process, another pager). The agent treated that as a footnote and treated "exactly-once" as a slogan. I am accepting the extra process because the unused Service Bus namespace is already the bill, and because a missing worker is a visible lag metric. A silent double-pay is not.

C would become interesting only if PayEx shipped a strongly consistent read-by-invoice-id inside the timeout window. They have not.

## Stress 2 — 03:00 on the last working day

Peak is "near 80 approvals a minute," and 60% of volume is in the last three working days. At 03:00 the worker is the thing that fails.

Failure I actually worry about: the worker dies, or Azure SQL hits DTU, or PayEx 30s timeouts pile Unknown rows. Approvals still return 200 if the outbox write committed. Finance sees a growing `outbox_lag`. That is the right failure. The wrong failure is the request thread retrying PayEx without a key because a dashboard said "Failed."

Runbook, not architecture: page on `outbox_lag > 5 minutes` or `unknown_rows > 50`. Do not page the person to "just rerun submit." Replay is `resume with the stored key`. If PayEx is dark until morning, we wait. We do not mint new keys.

C fails this night worse: the nightly CSV arrives after the business day starts. The on-call has nothing to say at 03:10 except "we'll know tomorrow."

## Stress 3 — cost to build and to run

Build, for a six-person team with two people new to the repo:

| Piece | Size I would defend | Why |
| --- | --- | --- |
| Azure SQL schema + EF (or Dapper) for invoice, audit, outbox | 4–6 engineering days | Replaces `InMemoryInvoiceStore`; this is required even without PayEx |
| Outbox writer in `InvoiceService.DecideAsync` | 1–2 days | One extra insert in the same transaction |
| Worker + PayEx client + key + Unknown handling | 4–6 days including tests for timeout and replay | This is the decision |
| Metrics, lag alert, replay command | 2 days | Useless without it |
| Nightly CSV reconcile | 1–2 days | Backstop |

Call it three weeks calendar if the two new engineers pair on the store swap and one veteran owns the worker. That is "cost to build" as time, not a vendor quote.

Run:

- App Service and Azure SQL we already owe for tier-2. The in-memory store cannot survive a recycle or a SOC 2 sample.
- Service Bus is already provisioned. Using it does not add a new product line; leaving it idle does not make B more expensive than A.
- Worker is another App Service plan or a Functions consumption app. At 40k invoices/month this is spare change next to one double-pay.
- PayEx call volume is one HTTP call per approval plus retries on Unknown. Retries are bounded by the key.

The agent produced dollar figures for Service Bus and SQL DTU without reading a bill. I deleted them. See the last section.

## What would make me revisit

1. PayEx publishes a consistent `GET /submissions?invoiceId=` that is correct inside the 30s window. Then A on the request path becomes simpler, though I would still keep the outbox for the SLO.
2. Outbox lag exceeds 15 minutes at month-end after we have tuned the worker. Then the design is wrong for this volume and we shard workers or drop to a pull model with more consumers — still the same key.
3. The SOC 2 auditor rejects "intent to pay" as the auditable event and demands the PayEx reference on the approve response. Then we have a product fight, not an ADR tweak. I would not put PayEx back on the 2s path without a written SLO change.
4. PayEx stops honouring idempotency keys. Then we stop submitting and we page them. We do not "just" switch to C.

## Where the agent was wrong or useless

- **It treated "exactly-once" as a property we can implement.** We can implement at-least-once delivery plus a key. PayEx implements the other half. The agent wrote "guarantee exactly once" in a first pass as if our process were the system of record for money. It is not.
- **It invented a PayEx get-by-id.** The evidence pack says there is no documented strongly consistent read. The agent still sketched a "check PayEx before retry" sequence. That sequence is fiction. I removed it from the target diagram.
- **It priced Azure.** The first cost section had monthly dollars for Service Bus and SQL. Those numbers were not from a quote, a calculator run, or our subscription. They were atmosphere. Useless. The useful cost is "we already provisioned Service Bus" and "one duplicate payment costs more than a Functions app."
- **It would not hold a preference under stress.** Asked to prefer C, it hedged instead of arguing C on C's strongest ground (ship date, no new process). A senior engineer who wanted C would say: "we ship SQL + fire-and-forget in January and accept T+1 because the alternative slips the cutover." That is a real position. The agent did not take it. I had to write it in so I could reject it.
- **It mixed as-built and target.** Early diagrams put PayEx on the current approve path. The code and the architecture test contradict that. The agent was reading `ops/README.md` as if it were a source map of `src/`.

The record above is mine. The agent was useful for listing A/B/C in one place and for naming the timeout contract. It was not useful for the decision.
