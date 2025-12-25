using MassTransit;
using OrchestratorService;
using Shared.Infrastructure.Logging;
using Serilog;
using Shared.Infrastructure.Extensions;

SerilogConfiguration.ConfigureLogging("OrchestratorService");

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// Configure MassTransit using Shared Extension
builder.Services.AddSharedMassTransit(builder.Configuration, x =>
{
    x.AddConsumer<SignalReceivedConsumer>();
});

var host = builder.Build();

try
{
    Log.Information("Starting OrchestratorService");
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "OrchestratorService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
