namespace InvoiceApproval.Api.Contracts;

public sealed record DecideInvoiceRequest(string Actor, string? Reason);
