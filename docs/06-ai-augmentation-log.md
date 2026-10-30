# AI augmentation log

Approved coding assistant, 2026-10-30, repository `ai-sldc-nagp-assignment`.

I used an assistant for first drafts and for running commands. I kept the decisions. This log is that split, not a claim that the assistant owns the service.

## What I used the assistant for

| Task | Assistant role | What I kept or changed |
| --- | --- | --- |
| Scenario facts | Pulled the PayEx, volume, and SOC 2 facts into a table | Kept in `ops/README.md`. |
| As-built vs target diagrams | First draft mixed Azure SQL and PayEx into the running process | I discarded that draft. Citations are from `src/` only. Inferences are listed separately. |
| Architecture test | Drafted `PersistenceBoundaryTests` | I kept the rule: request path goes through `IInvoiceStore`, no PayEx type. The command runs. |
| Exact-once options | Listed A, B, and C and favoured the outbox | I wrote the ADR. I deleted the Azure price list and the invented PayEx get-by-id. The "prefer C" stress did not move it off a hedge; I wrote the C argument so I could reject it. |
| Autonomy policy | Drafted autonomy levels | I cut it to month-end log triage and set day one to L1 / L0. |
| Review eval | Drafted cases, labels, prompts, and `run_eval.py` | I ran the set three times on v1 and once on v2. The reviewer is the prompt as a checklist. |
| Lethal trifecta | Named the three legs | I chose to remove outbound rather than accept the risk. |
| Measurement | Proposed queries | I ran `dotnet test` and the Actions API and recorded empty CI history as empty, not as 0%. |

## Where the assistant was wrong or useless

See the same heading in `docs/02-adr-001-exact-once-payex.md`.

- It read `ops/` plans as if they were the as-built architecture.
- It produced cloud cost numbers with no bill.
- Three runs of a deterministic checklist have variance 0. That is a real measurement. I did not treat it as three independent reviews.
- It was useful for listing options. It was not useful for the decision.
