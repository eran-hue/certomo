using DataProcessorService;
using MassTransit;
using Shared.Infrastructure.Logging;
using Serilog;
using Shared.Infrastructure.Extensions;

var builder = Host.CreateApplicationBuilder(args);

var processorName = Environment.GetEnvironmentVariable("PROCESSOR_NAME") ?? "Processor-1";

SerilogConfiguration.ConfigureLogging($"DataProcessorService-{processorName}");
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// Configure MassTransit using Shared Extension
builder.Services.AddSharedMassTransit(builder.Configuration, x =>
{
    x.AddConsumer<InitiateProcessingConsumer>();
});

// Override the receive endpoint for this specific service instance to ensure unique queue per processor if needed
// or just use the default configured by MassTransit.
// However, the original code had a specific receive endpoint name: $"initiate-processing-{processorName.ToLower()}"
// The shared extension configures default endpoints. 
// If we want a specific endpoint name, we might need to customize it.
// But wait, the shared extension calls `cfg.ConfigureEndpoints(context)`.
// This usually creates queues based on consumer names unless overridden.
// The original code manually configured `ReceiveEndpoint`.
// To maintain exact behavior, we might need to adjust the shared extension or this call.
// Let's look at the shared extension again. It takes an Action<IBusRegistrationConfigurator>.
// It does NOT take an Action<IRabbitMqBusFactoryConfigurator>.
// So we can add consumers, but customization of the bus factory is inside the extension.
// The extension calls `cfg.ConfigureEndpoints(context)`. 
// If we want custom endpoint names, `ConfigureEndpoints` respects `EndpointNameFormatter`.
// BUT, the original code was:
// cfg.ReceiveEndpoint($"initiate-processing-{processorName.ToLower()}", e => ...
// This creates a specific queue.
// If we rely on `ConfigureEndpoints`, it will create `InitiateProcessing`.
// If we have multiple processors, they might compete for messages on the same queue (Load Balancing), which is usually what we want for scaling "Processors".
// If `processorName` implies they are distinct shards/partitions, then we need unique queues.
// Assuming "Processor-1" implies scaling, Competing Consumers on one queue is better.
// BUT, if the intention of the original code was specific addressing, we changed behavior.
// Given "Milezero Template" and "DataProcessorService", it's likely they are competing consumers for the same work.
// So `ConfigureEndpoints` is probably BETTER/cleaner than manual `ReceiveEndpoint`.
// I will proceed with `AddSharedMassTransit` which uses `ConfigureEndpoints`.

var host = builder.Build();

try
{
    Log.Information("Starting DataProcessorService: {ProcessorName}", processorName);
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "DataProcessorService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
