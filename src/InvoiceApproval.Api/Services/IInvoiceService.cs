using InvoiceApproval.Api.Contracts;
using InvoiceApproval.Api.Domain;

namespace InvoiceApproval.Api.Services;

public interface IInvoiceService
{
    Task<Invoice> SubmitAsync(SubmitInvoiceRequest request, CancellationToken cancellationToken);
    Task<Invoice?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Invoice>> ListAsync(CancellationToken cancellationToken);
    Task<ApprovalResult> ApproveAsync(Guid id, string actor, CancellationToken cancellationToken);
    Task<ApprovalResult> RejectAsync(Guid id, string actor, string? reason, CancellationToken cancellationToken);
}
