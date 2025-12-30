using Microsoft.AspNetCore.Mvc;
using Shared.Application.DTOs;
using MediatR;
using ApiGateway.Features.Signals.SubmitSignal;
using Shared.Application.Behaviors;
using ApiGateway.Middleware;
using Shared.Infrastructure.Logging;
using Serilog;
using Shared.Infrastructure.Extensions;
using FluentValidation;

SerilogConfiguration.ConfigureLogging("ApiGateway");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddOpenApi();

// Register FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Register MediatR
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Register Shared Infrastructure
builder.Services.AddSharedInfrastructure(builder.Configuration);

// Configure MassTransit using Shared Extension
builder.Services.AddSharedMassTransit(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler(); // This must be first
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Serve static files (HTML/CSS/JS)

app.UseSerilogRequestLogging();

// Implement endpoint to accept signals
app.MapPost("/api/signals", async ([FromBody] SignalRequest request, IMediator mediator) =>
{
    var command = new SubmitSignalCommand(request.Value.ToString());
    var signalId = await mediator.Send(command);
    return Results.Accepted(value: new { SignalId = signalId });
})
.WithName("SubmitSignal");

// Endpoint to fetch Certomo Data
app.MapGet("/api/certomo-data", async (
    [FromServices] Shared.Application.Abstractions.ICertomoService certomoService, 
    [FromServices] Microsoft.Extensions.Configuration.IConfiguration configuration) =>
{
    try
    {
        var username = configuration["Certomo:Username"];
        var password = configuration["Certomo:Password"];
        var secret = configuration["Certomo:Secret"];

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(secret))
        {
            return Results.Problem("Missing Certomo credentials.");
        }

        var authResponse = await certomoService.AuthenticateAsync(username, password);
        var bankData = await certomoService.GetBankDataAsync(authResponse.Jwt, username, secret);

        return Results.Ok(bankData);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error fetching Certomo data");
        return Results.Problem("Failed to fetch data from Certomo.");
    }
})
.WithName("GetCertomoData");

try
{
    Log.Information("Starting ApiGateway");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ApiGateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

