using InvoiceApproval.Api.Endpoints;
using InvoiceApproval.Api.Persistence;
using InvoiceApproval.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddSingleton<IInvoiceStore, InMemoryInvoiceStore>();
builder.Services.AddSingleton<IInvoiceService, InvoiceService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    persistence = "in-memory",
    payex = "not-integrated"
}));
app.MapInvoiceEndpoints();

app.Run();

public partial class Program;
