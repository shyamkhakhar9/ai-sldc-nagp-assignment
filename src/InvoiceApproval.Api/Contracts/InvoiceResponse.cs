using InvoiceApproval.Api.Domain;

namespace InvoiceApproval.Api.Contracts;

public sealed record InvoiceResponse(
    Guid Id,
    string VendorId,
    string VendorName,
    decimal Amount,
    string Currency,
    string Description,
    string Status,
    string SubmittedBy,
    DateTimeOffset SubmittedAt,
    string? DecisionBy,
    DateTimeOffset? DecisionAt,
    string? RejectionReason)
{
    public static InvoiceResponse From(Invoice invoice) => new(
        invoice.Id,
        invoice.VendorId,
        invoice.VendorName,
        invoice.Amount,
        invoice.Currency,
        invoice.Description,
        invoice.Status.ToString(),
        invoice.SubmittedBy,
        invoice.SubmittedAt,
        invoice.DecisionBy,
        invoice.DecisionAt,
        invoice.RejectionReason);
}
