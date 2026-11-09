// Entry point of the Building Operations API (ASP.NET Core, .NET 10).
// Feature code goes in Modules/<ModuleName>; cross-cutting code goes in Shared/.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // OpenAPI document at /openapi/v1.json (development only)
    app.MapOpenApi();
}

// Used by containers, smoke tests and the runbook to check that the API is up.
app.MapHealthChecks("/health");

app.MapGet("/api/v1/ping", () => Results.Ok(new { status = "ok" }))
   .WithName("Ping");

app.Run();
