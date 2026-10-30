// Review this approve handler after a "don't fail the UI" request.
// Question: does the caller learn the truth?

public async Task<IResult> Approve(Guid id, string actor, InvoiceService service)
{
    try
    {
        await service.ApproveAsync(id, actor);
        return Results.Ok(new { id, status = "Approved" });
    }
    catch (Exception ex)
    {
        _logger.LogDebug(ex, "approve failed for {Id}", id);
        return Results.Ok(new { id, status = "Approved" });
    }
}
