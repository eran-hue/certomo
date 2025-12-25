using MassTransit;
using NotificationService;
using Shared.Infrastructure.Logging;
using Serilog;
using Shared.Infrastructure.Extensions;

SerilogConfiguration.ConfigureLogging("NotificationService");

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// Configure MassTransit using Shared Extension
builder.Services.AddSharedMassTransit(builder.Configuration, x =>
{
    x.AddConsumer<AggregationCompletedConsumer>();
});

var host = builder.Build();

try
{
    Log.Information("Starting NotificationService");
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "NotificationService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
