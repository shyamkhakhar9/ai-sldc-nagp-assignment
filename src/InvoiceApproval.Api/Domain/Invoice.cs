namespace InvoiceApproval.Api.Domain;

/// <summary>
/// Invoice as stored today. Persistence is in-memory; see <see cref="Persistence.InMemoryInvoiceStore"/>.
/// </summary>
public sealed class Invoice
{
    public required Guid Id { get; init; }
    public required string VendorId { get; init; }
    public required string VendorName { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required string Description { get; init; }
    public required string SubmittedBy { get; init; }
    public required DateTimeOffset SubmittedAt { get; init; }
    public InvoiceStatus Status { get; init; } = InvoiceStatus.Submitted;
    public string? DecisionBy { get; init; }
    public DateTimeOffset? DecisionAt { get; init; }
    public string? RejectionReason { get; init; }

    public Invoice WithDecision(InvoiceStatus status, string actor, string? reason) => new()
    {
        Id = Id,
        VendorId = VendorId,
        VendorName = VendorName,
        Amount = Amount,
        Currency = Currency,
        Description = Description,
        SubmittedBy = SubmittedBy,
        SubmittedAt = SubmittedAt,
        Status = status,
        DecisionBy = actor,
        DecisionAt = DateTimeOffset.UtcNow,
        RejectionReason = status == InvoiceStatus.Rejected ? reason : null
    };
}
