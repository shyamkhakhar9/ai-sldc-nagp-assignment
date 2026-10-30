# Platform

Planned runtime, not as-built:

- Azure App Service, Linux, ASP.NET Core 8.
- Azure SQL for invoices, decisions, and the seven-year audit table.
- GitHub Actions for build and test. The workflow in `.github/workflows/ci.yml` is the starting point.
- Azure Service Bus namespace is already provisioned in the subscription. No code in this repository sends or receives a message. That is unused capacity, not an existing outbox.

As-built runtime:

- Kestrel from `InvoiceApproval.Api`.
- `InMemoryInvoiceStore` (`src/InvoiceApproval.Api/Persistence/InMemoryInvoiceStore.cs`).
- No SQL client package, no Service Bus package, no PayEx client type.
