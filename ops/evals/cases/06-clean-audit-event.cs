// Review this audit event writer.
// Question: is the event itself defective?

public sealed record ApprovalAuditEvent(
    Guid InvoiceId,
    string PreviousStatus,
    string NewStatus,
    string Actor,
    DateTimeOffset AtUtc,
    string? PayExReference);

public static ApprovalAuditEvent Create(Invoice before, Invoice after, string actor) =>
    new(
        InvoiceId: after.Id,
        PreviousStatus: before.Status,
        NewStatus: after.Status,
        Actor: actor,
        AtUtc: DateTimeOffset.UtcNow,
        PayExReference: after.PayExReference);
