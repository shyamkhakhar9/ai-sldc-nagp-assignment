# AI augmentation log

Agent: Cursor agent (Composer), 2026-10-30, repository `ai-sldc-nagp-assignment`.

The original workshop codebase named in the brief (`ai-in-sdlc-assignment`) was not in this workspace. The empty GitHub repo `shyamkhakhar9/ai-sldc-nagp-assignment` was. The service, `ops/` evidence, and the ten review cases were reconstructed from the assignment PDF and then treated as the as-built system. That reconstruction is the largest use of the agent, and it is also the largest risk to the submission: I cannot prove these ten cases are the ten the session used.

## What the agent did

| Task | Agent role | What I kept / changed |
| --- | --- | --- |
| Read `AI-in-SDLC-Home Assignment-2.pdf` | Extracted the five tasks and the PayEx / volume / SOC 2 facts | Kept. These facts are in `ops/README.md`. |
| Search for `ai-in-sdlc-assignment` | Web and disk search. No public repo matched. | I stopped searching and built from the PDF rather than invent a fake clone URL. |
| As-built vs target diagrams | First draft mixed Azure SQL and PayEx into the current process | I threw that draft away. Citations are from `src/` only. Inferences are listed separately. |
| Architecture test | Agent wrote `PersistenceBoundaryTests` | I kept the rule (request path → `IInvoiceStore`, no PayEx type). Command is real. |
| Exact-once options | Agent listed A/B/C and favoured the outbox | I wrote the ADR. I deleted the agent's Azure price list and the invented PayEx get-by-id. The "prefer C" stress did not move the agent off a hedge; I wrote the C argument myself so I could reject it. |
| Autonomy policy | Agent drafted levels | I cut it to one workflow (month-end log triage) and stated day-one = L1/L0. |
| Review eval | Agent wrote cases, labels, prompts, and `run_eval.py` | I ran the script three times on v1 and once on v2. The reviewer is the prompt as a checklist, not a story. |
| Lethal trifecta | Agent named all three legs | I chose remove-outbound rather than accept-the-risk. |
| Measurement | Agent proposed queries | I ran `dotnet test` and `gh run list` and recorded empty CI history as empty, not as 0%. |

## Where the agent was wrong or useless

See also the same heading in `docs/02-adr-001-exact-once-payex.md`.

- It cannot see a workshop repo that is not on disk. Building a substitute is useful for a working service and dishonest if the grader expected the session's exact `ops/evals` files.
- It read `ops/` plans as if they were as-built architecture.
- It produced cost numbers with no bill.
- Three runs of a deterministic checklist have variance 0. That is a real measurement and a boring one. I did not pretend a single model produced three independent human reviews.

## What I did not ask the agent to do

- Record the 8–10 minute demo. A script is in `docs/demo-script.md`. The MP4 is a human deliverable.
- Push to GitHub or open a pull request, unless asked.
- Invent merged-PR lead times.
