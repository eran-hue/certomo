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

app.UseSerilogRequestLogging();

// Implement endpoint to accept signals
app.MapPost("/api/signals", async ([FromBody] SignalRequest request, IMediator mediator) =>
{
    var command = new SubmitSignalCommand(request.Value.ToString());
    var signalId = await mediator.Send(command);
    return Results.Accepted(value: new { SignalId = signalId });
})
.WithName("SubmitSignal");

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

