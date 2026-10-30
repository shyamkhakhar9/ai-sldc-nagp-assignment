using InvoiceApproval.Api.Contracts;
using InvoiceApproval.Api.Domain;
using InvoiceApproval.Api.Services;

namespace InvoiceApproval.Api.Endpoints;

/// <summary>
/// HTTP surface for submit / list / get / approve / reject.
/// As-built there is no authentication. That is an internal-tool leftover, not a target-state choice.
/// </summary>
public static class InvoiceEndpoints
{
    public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices");

        group.MapPost("/", SubmitAsync);
        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", GetAsync);
        group.MapPost("/{id:guid}/approve", ApproveAsync);
        group.MapPost("/{id:guid}/reject", RejectAsync);

        return app;
    }

    private static async Task<IResult> SubmitAsync(
        SubmitInvoiceRequest request,
        IInvoiceService invoices,
        CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await invoices.SubmitAsync(request, cancellationToken).ConfigureAwait(false);
            return Results.Created($"/api/invoices/{invoice.Id}", InvoiceResponse.From(invoice));
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListAsync(IInvoiceService invoices, CancellationToken cancellationToken)
    {
        var items = await invoices.ListAsync(cancellationToken).ConfigureAwait(false);
        return Results.Ok(items.Select(InvoiceResponse.From));
    }

    private static async Task<IResult> GetAsync(Guid id, IInvoiceService invoices, CancellationToken cancellationToken)
    {
        var invoice = await invoices.GetAsync(id, cancellationToken).ConfigureAwait(false);
        return invoice is null
            ? Results.NotFound(new { error = "Invoice not found." })
            : Results.Ok(InvoiceResponse.From(invoice));
    }

    private static async Task<IResult> ApproveAsync(
        Guid id,
        DecideInvoiceRequest request,
        IInvoiceService invoices,
        CancellationToken cancellationToken)
    {
        var result = await invoices.ApproveAsync(id, request.Actor, cancellationToken).ConfigureAwait(false);
        return ToHttp(result);
    }

    private static async Task<IResult> RejectAsync(
        Guid id,
        DecideInvoiceRequest request,
        IInvoiceService invoices,
        CancellationToken cancellationToken)
    {
        var result = await invoices.RejectAsync(id, request.Actor, request.Reason, cancellationToken)
            .ConfigureAwait(false);
        return ToHttp(result);
    }

    private static IResult ToHttp(ApprovalResult result) => result.Outcome switch
    {
        ApprovalOutcome.Approved or ApprovalOutcome.Rejected when result.Invoice is not null
            => Results.Ok(InvoiceResponse.From(result.Invoice)),
        ApprovalOutcome.NotFound => Results.NotFound(new { error = result.Error }),
        ApprovalOutcome.Conflict => Results.Conflict(new { error = result.Error }),
        _ => Results.Problem(result.Error)
    };
}
