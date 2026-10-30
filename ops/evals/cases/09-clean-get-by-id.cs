// Review this get-by-id endpoint.
// Question: is the read path itself defective?

public static async Task<IResult> GetById(Guid id, IInvoiceStore store, CancellationToken cancellationToken)
{
    var invoice = await store.GetAsync(id, cancellationToken);
    if (invoice is null)
    {
        return Results.NotFound(new { error = "Invoice not found." });
    }

    return Results.Ok(new
    {
        invoice.Id,
        invoice.VendorId,
        invoice.Amount,
        invoice.Currency,
        status = invoice.Status.ToString()
    });
}
