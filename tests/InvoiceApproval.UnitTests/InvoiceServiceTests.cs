using InvoiceApproval.Api.Contracts;
using InvoiceApproval.Api.Domain;
using InvoiceApproval.Api.Persistence;
using InvoiceApproval.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace InvoiceApproval.UnitTests;

public sealed class InvoiceServiceTests
{
    private static InvoiceService CreateService(IInvoiceStore? store = null) =>
        new(store ?? new InMemoryInvoiceStore(), NullLogger<InvoiceService>.Instance);

    [Fact]
    public async Task Submit_then_approve_returns_approved_invoice()
    {
        var service = CreateService();
        var submitted = await service.SubmitAsync(ValidRequest(), CancellationToken.None);

        var result = await service.ApproveAsync(submitted.Id, "alex", CancellationToken.None);

        Assert.Equal(ApprovalOutcome.Approved, result.Outcome);
        Assert.Equal(InvoiceStatus.Approved, result.Invoice!.Status);
        Assert.Equal("alex", result.Invoice.DecisionBy);
        Assert.NotNull(result.Invoice.DecisionAt);
    }

    [Fact]
    public async Task Second_approve_is_a_conflict()
    {
        var service = CreateService();
        var submitted = await service.SubmitAsync(ValidRequest(), CancellationToken.None);
        await service.ApproveAsync(submitted.Id, "alex", CancellationToken.None);

        var second = await service.ApproveAsync(submitted.Id, "blair", CancellationToken.None);

        Assert.Equal(ApprovalOutcome.Conflict, second.Outcome);
    }

    [Fact]
    public async Task Approve_of_unknown_invoice_is_not_found()
    {
        var service = CreateService();

        var result = await service.ApproveAsync(Guid.NewGuid(), "alex", CancellationToken.None);

        Assert.Equal(ApprovalOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task Submit_rejects_non_positive_amount()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SubmitAsync(ValidRequest() with { Amount = 0 }, CancellationToken.None));
    }

    private static SubmitInvoiceRequest ValidRequest() => new(
        VendorId: "V-100",
        VendorName: "Northwind Paper",
        Amount: 1250.50m,
        Currency: "usd",
        Description: "Q4 paper stock",
        SubmittedBy: "casey");
}
