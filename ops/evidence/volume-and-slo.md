# Volume and SLO

- About 40,000 invoices a month.
- About 60% arrive in the last three working days.
- Peak observed on the internal tool: near 80 approvals a minute.
- Draft SLO for January: 99.5% of `POST /api/invoices/{id}/approve` complete in 2 seconds, measured at the App Service gateway. PayEx submission is **not** on that request path in the target design (see the outbox decision record).
- Error budget is consumed by 5xx and by approvals that return success without a durable write.

Month-end is the load that will find a missing unique index or a worker that cannot keep up. It is also the window where a duplicate PayEx submission is most expensive.
