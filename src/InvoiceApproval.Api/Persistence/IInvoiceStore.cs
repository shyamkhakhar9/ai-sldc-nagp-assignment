using InvoiceApproval.Api.Domain;

namespace InvoiceApproval.Api.Persistence;

/// <summary>
/// The only persistence boundary in this service. Today the implementation is in-memory.
/// Azure SQL is part of the planned deployment, not of the running code.
/// </summary>
public interface IInvoiceStore
{
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken);
    Task<Invoice?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Invoice>> ListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Compare-and-swap on <see cref="Invoice.Status"/>. Returns false when the stored
    /// status is not <paramref name="expectedStatus"/> or the invoice is missing.
    /// </summary>
    Task<bool> TryUpdateAsync(Invoice invoice, InvoiceStatus expectedStatus, CancellationToken cancellationToken);
}
