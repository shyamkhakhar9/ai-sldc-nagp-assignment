// Review this PayEx client constructor.
// Question: is the credential handling acceptable?

public sealed class PayExClient
{
    public PayExClient(HttpClient http)
    {
        _http = http;
        _http.DefaultRequestHeaders.Add("X-Api-Key", "px_live_4f8a21c9e0b77d13");
    }

    private readonly HttpClient _http;
}
