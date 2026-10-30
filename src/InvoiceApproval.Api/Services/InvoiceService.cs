using InvoiceApproval.Api.Contracts;
using InvoiceApproval.Api.Domain;
using InvoiceApproval.Api.Persistence;

namespace InvoiceApproval.Api.Services;

/// <summary>
/// Approval use-cases. As-built this updates the in-memory store and stops.
/// PayEx submission is not in this path; see docs/02-adr-001-exact-once-payex.md.
/// </summary>
public sealed class InvoiceService : IInvoiceService
{
    private readonly IInvoiceStore _store;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(IInvoiceStore store, ILogger<InvoiceService> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task<Invoice> SubmitAsync(SubmitInvoiceRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.VendorId))
        {
            throw new ArgumentException("VendorId is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.VendorName))
        {
            throw new ArgumentException("VendorName is required.", nameof(request));
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Length != 3)
        {
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.SubmittedBy))
        {
            throw new ArgumentException("SubmittedBy is required.", nameof(request));
        }

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            VendorId = request.VendorId.Trim(),
            VendorName = request.VendorName.Trim(),
            Amount = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero),
            Currency = request.Currency.Trim().ToUpperInvariant(),
            Description = request.Description?.Trim() ?? string.Empty,
            SubmittedBy = request.SubmittedBy.Trim(),
            SubmittedAt = DateTimeOffset.UtcNow,
            Status = InvoiceStatus.Submitted
        };

        await _store.AddAsync(invoice, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Invoice {InvoiceId} submitted by {Actor} for {Amount} {Currency}",
            invoice.Id, invoice.SubmittedBy, invoice.Amount, invoice.Currency);
        return invoice;
    }

    public Task<Invoice?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        _store.GetAsync(id, cancellationToken);

    public Task<IReadOnlyList<Invoice>> ListAsync(CancellationToken cancellationToken) =>
        _store.ListAsync(cancellationToken);

    public Task<ApprovalResult> ApproveAsync(Guid id, string actor, CancellationToken cancellationToken) =>
        DecideAsync(id, actor, InvoiceStatus.Approved, reason: null, cancellationToken);

    public Task<ApprovalResult> RejectAsync(Guid id, string actor, string? reason, CancellationToken cancellationToken) =>
        DecideAsync(id, actor, InvoiceStatus.Rejected, reason, cancellationToken);

    private async Task<ApprovalResult> DecideAsync(
        Guid id,
        string actor,
        InvoiceStatus nextStatus,
        string? reason,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return new ApprovalResult(ApprovalOutcome.Conflict, null, "Actor is required.");
        }

        var current = await _store.GetAsync(id, cancellationToken).ConfigureAwait(false);
        if (current is null)
        {
            return new ApprovalResult(ApprovalOutcome.NotFound, null, "Invoice not found.");
        }

        if (current.Status != InvoiceStatus.Submitted)
        {
            return new ApprovalResult(ApprovalOutcome.Conflict, current, $"Invoice is already {current.Status}.");
        }

        var decided = current.WithDecision(nextStatus, actor.Trim(), reason);
        var updated = await _store.TryUpdateAsync(decided, InvoiceStatus.Submitted, cancellationToken)
            .ConfigureAwait(false);
        if (!updated)
        {
            var latest = await _store.GetAsync(id, cancellationToken).ConfigureAwait(false);
            return new ApprovalResult(ApprovalOutcome.Conflict, latest, "Invoice was decided by another actor.");
        }

        _logger.LogInformation("Invoice {InvoiceId} {Decision} by {Actor}", id, nextStatus, actor);
        return new ApprovalResult(
            nextStatus == InvoiceStatus.Approved ? ApprovalOutcome.Approved : ApprovalOutcome.Rejected,
            decided,
            null);
    }
}
