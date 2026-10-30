# Demo script (8–10 minutes)

Record this as an MP4 (Teams or any screen recorder). Target 8–10 minutes. Do not skip the eval table or the "agent was wrong" page.

| Min | Show | Say |
| --- | --- | --- |
| 0:00–0:45 | `ops/README.md` facts table | I am the senior engineer for Invoice Approval. Today it is an in-memory internal tool. January it is tier-2 with PayEx. Ops is the plan; `src/` is the as-built. |
| 0:45–2:15 | `docs/01-as-built-and-target-diagrams.md` | Walk the as-built container and the approve sequence. Point at each citation. Read one inferred item that was wrong (PayEx on the current path). Show the target diagram and name what is `<<planned>>`. |
| 2:15–3:00 | Terminal | Run `dotnet test tests/InvoiceApproval.ArchitectureTests/InvoiceApproval.ArchitectureTests.csproj`. One structural rule, it actually runs. |
| 3:00–5:00 | `docs/02-adr-001-exact-once-payex.md` | Three options. Decision is outbox + stored idempotency key. Stress: preferring C did not produce a real argument; 03:00 month-end fails as lag not as double-pay; cost is time and an already-provisioned Service Bus, not invented dollars. Read "Where the agent was wrong or useless." |
| 5:00–6:00 | `docs/03-autonomy-policy.md` | Month-end log triage. Day one is L1 draft / L0 for Slack and PayEx because two engineers are new. Replay of PayEx stays L0. |
| 6:00–7:30 | `docs/04-review-evaluation.md` and `ops/evals/results/` | Ten fixed cases, two added (local `DateTime.Now`; clean cancellation tokens). Three v1 runs, same numbers. v2 adds one checklist item. Say whether it improved and how little confidence n=12 buys. |
| 7:30–8:45 | `docs/05-risk-audit-and-measurement.md` | All three trifecta legs were in the planned agent session. Decision: remove outbound. Show the two captured numbers: test pass count, zero GitHub Actions runs. |
| 8:45–9:30 | `docs/06-ai-augmentation-log.md` | What the agent did, what I threw away, what I would not let it invent. |
| 9:30–10:00 | Stop | No extra slides. If time remains, rerun `python3 ops/evals/run_eval.py --prompt v1 --run-id demo`. |

Upload the MP4 and put the link in the root `README.md`.
