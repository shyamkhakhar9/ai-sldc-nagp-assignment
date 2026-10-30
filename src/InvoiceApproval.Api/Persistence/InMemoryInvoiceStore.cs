using System.Collections.Concurrent;
using InvoiceApproval.Api.Domain;

namespace InvoiceApproval.Api.Persistence;

/// <summary>
/// As-built persistence. Process recycle loses every invoice. This is the fact the
/// January tier-2 cutover has to replace, not a production store.
/// </summary>
public sealed class InMemoryInvoiceStore : IInvoiceStore
{
    private readonly ConcurrentDictionary<Guid, Invoice> _invoices = new();

    public Task AddAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        if (!_invoices.TryAdd(invoice.Id, invoice))
        {
            throw new InvalidOperationException($"Invoice {invoice.Id} already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<Invoice?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        _invoices.TryGetValue(id, out var invoice);
        return Task.FromResult(invoice);
    }

    public Task<IReadOnlyList<Invoice>> ListAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Invoice> snapshot = _invoices.Values
            .OrderByDescending(i => i.SubmittedAt)
            .ToList();
        return Task.FromResult(snapshot);
    }

    public Task<bool> TryUpdateAsync(Invoice invoice, InvoiceStatus expectedStatus, CancellationToken cancellationToken)
    {
        if (!_invoices.TryGetValue(invoice.Id, out var current))
        {
            return Task.FromResult(false);
        }

        if (current.Status != expectedStatus)
        {
            return Task.FromResult(false);
        }

        _invoices[invoice.Id] = invoice;
        return Task.FromResult(true);
    }
}
