// Review this approve implementation.
// Question: is the status transition safe at 80 approvals a minute?

public async Task<bool> ApproveAsync(Guid id, string actor)
{
    var invoice = await _db.Invoices.FindAsync(id);
    if (invoice is null || invoice.Status != "Submitted")
    {
        return false;
    }

    invoice.Status = "Approved";
    invoice.ApprovedBy = actor;
    invoice.ApprovedAt = DateTimeOffset.UtcNow;
    await _db.SaveChangesAsync();
    return true;
}
