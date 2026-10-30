// Added case (real defect). A service I have seen stamped audit rows with DateTime.Now.
// At month-end a node in IST and a node in UTC wrote overlapping "same hour" rows.
// SOC 2 reconstruction then disagreed about who approved first.
// Question: is this timestamp acceptable for a seven-year approval audit?

public void RecordApproval(Guid invoiceId, string actor)
{
    _audit.Insert(new ApprovalRow
    {
        InvoiceId = invoiceId,
        Actor = actor,
        ApprovedAt = DateTime.Now,
        DayBucket = DateTime.Now.ToString("yyyy-MM-dd")
    });
}
