namespace InvoiceApproval.Api.Contracts;

public sealed record SubmitInvoiceRequest(
    string VendorId,
    string VendorName,
    decimal Amount,
    string Currency,
    string Description,
    string SubmittedBy);
