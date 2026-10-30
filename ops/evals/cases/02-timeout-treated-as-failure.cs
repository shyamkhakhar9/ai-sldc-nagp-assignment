// Review this PayEx submission helper.
// Question: does this treat PayEx's contract correctly?

public async Task SubmitApprovedAsync(Invoice invoice)
{
    try
    {
        var response = await _http.PostAsJsonAsync(
            "https://payex.example/submissions",
            new { invoiceId = invoice.Id, amount = invoice.Amount });
        response.EnsureSuccessStatusCode();
        invoice.PayExReference = await response.Content.ReadFromJsonAsync<string>();
        invoice.SubmissionState = "Sent";
    }
    catch (TaskCanceledException)
    {
        // Timed out — PayEx did not accept it. Safe to retry later.
        invoice.SubmissionState = "Failed";
        await _queue.EnqueueAsync(invoice);
    }
}
