# Autonomy policy — month-end log triage

One page. Workflow: the last six hours of Invoice Approval logs during the last three working days, when 60% of the month arrives and the pager is most likely to ring.

Levels used here, and only these:

| Level | Meaning |
| --- | --- |
| L0 | Human reads the logs. Agent is off. |
| L1 | Agent drafts a grouping. Human decides what is real and what to do. Agent cannot post or change systems. |
| L2 | Agent drafts and proposes the next action (restart worker, replay outbox row, open an incident). A named human approves before anything leaves the laptop. |
| L3 | Agent posts the summary to Slack and opens the incident ticket. Human reviews after. |
| L4 | Agent pages, posts, and replays outbox rows without asking. |

## Which tasks sit where

| Task | Level | Why this and not higher |
| --- | --- | --- |
| Parse the six-hour dump, cluster by exception type and route | L1 on day one, L2 after evidence | Cheap if wrong. Wrong clusters waste minutes, not money. |
| Name the likely blast radius ("approves succeeding, PayEx worker stalled") | L1 | Needs the outbox lag number. The agent may not have it. |
| Post a summary to `#invoice-oncall` | L2 | Slack is an exfiltration path (see the risk audit). A human hits send. |
| Open or update a Sev-2 incident | L2 | Public ticket text plus private invoice ids is how we leak payee data. |
| Restart the outbox worker or bounce App Service | L2, never L3 in year one | Restart is reversible; doing it on a healthy node during peak is a self-inflicted outage. |
| Replay an outbox row / call PayEx | L0 unless a written replay command exists with the stored idempotency key | This is money. The ADR forbids minting a new key. An agent that "just retries" is option C in disguise. |
| Change feature flags or connection strings | L0 | Out of scope for triage. |

## Controls that make the level safe

- The triage session **cannot** have Slack, browser MCP, and Azure SQL in the same chat. Day-one L1 uses a redacted log export (no request bodies, no IBANs) pasted by the human. That is the redesigned agent setup in `docs/05-risk-audit-and-measurement.md`.
- The draft is a file or a sidebar, not a webhook. L2 send is a human click on a rendered summary that has already stripped account numbers.
- Replay is a checked-in command: `dotnet run -- replay --invoice <id>` uses the key in the outbox row. There is no "retry PayEx" prompt.
- Every L2 action records actor, time (UTC), and the draft the agent produced. That is the seven-year trail applied to operations, not only to approvals.

## Evidence to move a task up a level

Move cluster-and-draft from L1 to L2 when all of the following are true, measured, not hoped:

1. Twenty consecutive month-end or rehearsal dumps where the human accepted the grouping with at most one wrong cluster.
2. Zero incidents in that window where the draft omitted a 5xx class that the human later found.
3. The redaction filter has a test: a planted IBAN in a log line does not appear in the draft.

Move Slack-post from L2 to L3 only after those three, plus a week of L2 posts with no accidental PII and a written allow-list of fields (invoice id, route, count, first/last UTC timestamp). Amounts stay out of Slack unless a human types them.

Never move PayEx replay above L0 in this policy. That is a different workflow with a different ADR.

## Who approves a move

- L1 → L2 for clustering: the senior engineer for this service, after the twenty-dump sheet is attached to the change.
- L2 → L3 for Slack: the same senior engineer **and** the engineer who owns SOC 2 evidence for the team. Either can refuse.
- Any proposal to give the agent a PayEx credential or a replay tool: refused under this policy. Open a new record.

## Day-one level for this rotation

**L1 for clustering and drafting. L0 for everything that leaves the machine or touches PayEx.**

Two of the six engineers are new to this repository (`ops/evidence/team-and-rotation.md`). The person who has the pager in week two may be one of them. L2 assumes the human can see that the draft is wrong. That is a bad assumption on day one. L1 still saves the new engineer from grepping six hours of logs by hand. It does not let the agent speak to Slack or to PayEx while they are still learning what an Unknown outbox row means.
