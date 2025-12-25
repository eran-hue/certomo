using MassTransit;
using Shared.Core.Events;

namespace NotificationService;

public class AggregationCompletedConsumer : IConsumer<AggregationCompleted>
{
    private readonly ILogger<AggregationCompletedConsumer> _logger;

    public AggregationCompletedConsumer(ILogger<AggregationCompletedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<AggregationCompleted> context)
    {
        _logger.LogInformation("Notification Sent: Signal {SignalId} processed successfully. Final Result: {Result}", context.Message.SignalId, context.Message.FinalResult);
        
        // Simulate sending email/SMS
        Console.WriteLine($"[NOTIFICATION] Signal {context.Message.SignalId} completed with result {context.Message.FinalResult}.");

        return Task.CompletedTask;
    }
}
