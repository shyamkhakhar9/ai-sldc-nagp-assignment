# Invoice Approval — AI in SDLC assignment

Senior-engineer pack for the Invoice Approval service before it becomes tier-2: an as-built diagram that matches the code, a decision record for PayEx, an autonomy policy, a review evaluation that actually ran, and a measurement plan with two captured numbers.

Working codebase and hand-in for the Invoice Approval tier-2 cutover.

## Hand-in index

| # | Deliverable | Where |
| --- | --- | --- |
| 1 | Mermaid source, citations, inferred-items list | `diagrams/*.mmd`, `docs/01-as-built-and-target-diagrams.md` |
| 2 | One architecture test and the command that runs it | `tests/InvoiceApproval.ArchitectureTests/PersistenceBoundaryTests.cs` — command below |
| 3 | Exact-once decision record, including *Where the agent was wrong or useless* | `docs/02-adr-001-exact-once-payex.md` |
| 4 | One-page autonomy policy, day-one level stated | `docs/03-autonomy-policy.md` |
| 5 | Two added eval cases and the results table (3× + changed-variable) | `ops/evals/cases/11-*`, `ops/evals/cases/12-*`, `docs/04-review-evaluation.md` |
| 6 | Risk audit with a recorded decision | `docs/05-risk-audit-and-measurement.md` |
| 7 | Measurement plan and the two numbers captured today | same file, plus `docs/measurements/` |
| 8 | 8–10 minute demo video | Script: `docs/demo-script.md`. Link: *add after you upload the MP4* |
| 9 | AI augmentation log | `docs/06-ai-augmentation-log.md` |

## Architecture test command

```bash
dotnet test tests/InvoiceApproval.ArchitectureTests/InvoiceApproval.ArchitectureTests.csproj
```

## As-built service

ASP.NET Core 8 minimal API. In-memory store. No PayEx client.

```bash
dotnet test InvoiceApproval.sln
dotnet run --project src/InvoiceApproval.Api
```

- `POST /api/invoices`
- `GET /api/invoices` and `GET /api/invoices/{id}`
- `POST /api/invoices/{id}/approve` and `.../reject`
- `GET /health` → `{ persistence: in-memory, payex: not-integrated }`

Planned January operations (not this process) are in `ops/README.md`.

## Review evaluation command

```bash
python3 ops/evals/run_eval.py --prompt v1 --run-id r1
python3 ops/evals/run_eval.py --prompt v1 --run-id r2
python3 ops/evals/run_eval.py --prompt v1 --run-id r3
python3 ops/evals/run_eval.py --prompt v2 --run-id r4-prompt-change
```

## Demo video

_Add the uploaded MP4 URL here after recording._
