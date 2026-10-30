namespace InvoiceApproval.Api.Domain;

public enum ApprovalOutcome
{
    Approved,
    Rejected,
    NotFound,
    Conflict
}

public sealed record ApprovalResult(ApprovalOutcome Outcome, Invoice? Invoice, string? Error);
