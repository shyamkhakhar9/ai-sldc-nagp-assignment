// Added case (clean). Pattern taken from a service that did this right:
// every I/O method takes the request CancellationToken and does not create a linked
// timeout that is shorter than PayEx's 30s window on the request thread.
// Question: is this snippet defective?

public sealed class InvoiceReader
{
    private readonly IInvoiceStore _store;

    public InvoiceReader(IInvoiceStore store) => _store = store;

    public Task<Invoice?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        _store.GetAsync(id, cancellationToken);

    public Task<IReadOnlyList<Invoice>> ListAsync(CancellationToken cancellationToken) =>
        _store.ListAsync(cancellationToken);
}
