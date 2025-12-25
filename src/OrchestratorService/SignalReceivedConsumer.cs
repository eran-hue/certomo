using MassTransit;
using Shared.Core.Events;

namespace OrchestratorService;

public class SignalReceivedConsumer : IConsumer<SignalReceived>
{
    private readonly ILogger<SignalReceivedConsumer> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public SignalReceivedConsumer(ILogger<SignalReceivedConsumer> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<SignalReceived> context)
    {
        _logger.LogInformation("Signal Received: {SignalId}, Value: {Value}", context.Message.SignalId, context.Message.Value);
        
        // Dispatch tasks to Data Processors
        await _publishEndpoint.Publish<InitiateProcessing>(new 
        {
            SignalId = context.Message.SignalId,
            Value = context.Message.Value,
            Timestamp = DateTime.UtcNow
        });
        
        _logger.LogInformation("InitiateProcessing Event Published: {SignalId}", context.Message.SignalId);
    }
}
