# Invoice Approval — planned operations

This folder describes the **January tier-2 deployment**, not the code that runs today.

The service has been an internal tool for 18 months, maintained part-time by one person. Persistence is an in-memory dictionary. There is no payment provider. In January it becomes a tier-2 service: a published SLO, a one-week on-call rotation, Azure SQL, and a PayEx submission for every approved invoice.

Keep the distinction. Diagrams that mix the two are wrong.

## Facts used by the assignment

| Fact | Detail | Evidence |
| --- | --- | --- |
| Team | Six engineers, two new to this codebase, one-week on-call rotation | `evidence/team-and-rotation.md` |
| Platform | ASP.NET Core 8 on Azure App Service, Azure SQL, GitHub Actions | `evidence/platform.md` |
| Unused capacity | Azure Service Bus is provisioned. Nothing in the repository publishes or consumes a message. | `evidence/platform.md` |
| Volume | ~40,000 invoices a month. 60% land in the last three working days. Peak near 80 approvals a minute. | `evidence/volume-and-slo.md` |
| Persistence today | None. `InMemoryInvoiceStore` is the store. Azure SQL is part of going to production. | this file, and `src/InvoiceApproval.Api/Persistence/InMemoryInvoiceStore.cs` |
| PayEx | Returns a reference on success. Times out at 30 seconds. A timeout does not reliably mean the submission failed. | `evidence/payex-behavior.md` |
| Compliance | SOC 2 Type II in six months. Approval actions auditable for seven years. | `evidence/compliance.md` |

## What the running code actually does

- Submit, list, get, approve, reject invoices over HTTP.
- Store them in a process-local `ConcurrentDictionary`.
- Write information logs for submit and decide.
- Expose `/health` that reports `persistence=in-memory` and `payex=not-integrated`.

It does not talk to Azure SQL, Service Bus, or PayEx. Those names belong to the target state.

## Evidence packs

- `evidence/team-and-rotation.md`
- `evidence/platform.md`
- `evidence/volume-and-slo.md`
- `evidence/payex-behavior.md`
- `evidence/compliance.md`

## Review evaluation pack

Ten fixed cases live in `evals/cases/01` through `evals/cases/10`. Labels are in `evals/labels.json`. Two extra cases (`11`, `12`) were added for this submission. How to run the set is in `evals/README.md`.

## Agent setup (as planned for the rotation)

This is the setup the risk audit in `docs/05-risk-audit-and-measurement.md` is scored against.

The on-call channel has been piloting a repo-scoped coding agent (Cursor / Copilot-style) with the following tools enabled in one session:

1. **Repository read/write** on `ai-sldc-nagp-assignment`, including this `ops/` tree and `.env` samples.
2. **Azure SQL read** through a stored connection string, so the agent can `SELECT` invoices, approval decisions, and vendor bank details while triaging month-end failures.
3. **Log Analytics / Application Insights** for the last six hours of request logs. Those logs include raw request bodies from `/api/invoices` (vendor-supplied memo text and, in some environments, uploaded attachment metadata).
4. **Blob read** on the invoice-attachment container. Vendors upload PDFs. The agent is asked to "open the attachment if the log line is unclear."
5. **Slack incoming webhook** to `#invoice-oncall`. The first-response runbook says: dump a summary, include invoice ids, amounts, and a link to the PayEx dashboard.
6. **Browser MCP** allowed to open vendor portals and `https://payex.example` status pages, including pages the model is told to "fetch and summarise."
7. **GitHub write**: open issues, comment on issues (public and private), and open pull requests.

The month-end log-triage workflow pastes the last six hours of logs into the agent and says "group the failures and post the summary to Slack." That prompt is the one the risk audit treats as the default agent session.

The lethal-trifecta audit and the redesigned setup are in `docs/05-risk-audit-and-measurement.md`.
