// Review this change to the approval endpoint.
// Question: is this safe to merge for a tier-2 invoice service?

app.MapPost("/api/invoices/{id:guid}/approve", async (Guid id, InvoiceStore store) =>
{
    var invoice = store.Get(id);
    if (invoice is null)
    {
        return Results.NotFound();
    }

    invoice.Status = "Approved";
    invoice.ApprovedAt = DateTime.UtcNow;
    store.Save(invoice);
    return Results.Ok(invoice);
});
