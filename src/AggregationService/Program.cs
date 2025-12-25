using AggregationService;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.Logging;
using Serilog;
using Shared.Infrastructure.Extensions;
using MassTransit;

SerilogConfiguration.ConfigureLogging("AggregationService");

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// Add Shared Infrastructure (Database, etc.)
builder.Services.AddSharedInfrastructure(builder.Configuration);

// Configure MassTransit using Shared Extension
builder.Services.AddSharedMassTransit(builder.Configuration, x =>
{
    x.AddConsumer<DataProcessedConsumer>();
});

var host = builder.Build();

// Ensure database is created
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OperationResilientDbContext>();
    db.Database.Migrate();
}

try
{
    Log.Information("Starting AggregationService");
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AggregationService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
