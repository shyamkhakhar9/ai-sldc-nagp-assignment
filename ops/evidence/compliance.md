# Compliance

- SOC 2 Type II audit in six months.
- Approval actions must be reconstructable for seven years: who decided, when (UTC), which invoice, previous status, new status, and the PayEx reference once submissions exist.
- The in-memory store cannot satisfy this. Process recycle is data loss, not an audit strategy.
- Agent sessions that can read invoices and post to Slack are in scope for the audit if they become the on-call default. See the agent setup at the end of `ops/README.md`.
