using MassTransit;
using Shared.Core.Events;

namespace DataProcessorService;

public class InitiateProcessingConsumer : IConsumer<InitiateProcessing>
{
    private readonly ILogger<InitiateProcessingConsumer> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly string _processorName;

    public InitiateProcessingConsumer(ILogger<InitiateProcessingConsumer> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _processorName = Environment.GetEnvironmentVariable("PROCESSOR_NAME") ?? "Processor-1";
    }

    public async Task Consume(ConsumeContext<InitiateProcessing> context)
    {
        _logger.LogInformation("{ProcessorName} Started: {SignalId}", _processorName, context.Message.SignalId);

        // Simulate processing
        var random = new Random();
        int delay = random.Next(100, 1000);
        await Task.Delay(delay); // Simulate work

        // Failure Simulation: 20% chance of failure
        if (random.Next(1, 101) <= 20)
        {
            _logger.LogError("{ProcessorName} Failed for Signal: {SignalId}", _processorName, context.Message.SignalId);
            throw new Exception($"Simulated processing failure for Signal {context.Message.SignalId} in {_processorName}");
        }

        // Process data (e.g., multiply by 2 for this processor)
        int processedValue = context.Message.Value * 2;

        await _publishEndpoint.Publish<DataProcessed>(new 
        {
            SignalId = context.Message.SignalId,
            ProcessorName = _processorName,
            ProcessedValue = processedValue,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("{ProcessorName} Completed: {SignalId}, Result: {Result}", _processorName, context.Message.SignalId, processedValue);
    }
}
