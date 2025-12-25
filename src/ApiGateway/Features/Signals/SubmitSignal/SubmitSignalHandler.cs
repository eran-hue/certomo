namespace ApiGateway.Features.Signals.SubmitSignal;

using MediatR;
using MassTransit;
using Shared.Application.Abstractions;
using Shared.Core.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

public class SubmitSignalHandler : IRequestHandler<SubmitSignalCommand, Guid>
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<SubmitSignalHandler> _logger;

    public SubmitSignalHandler(IMessageBus messageBus, ILogger<SubmitSignalHandler> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task<Guid> Handle(SubmitSignalCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling SubmitSignalCommand");
        
        var signalId = Guid.NewGuid();
        if (!int.TryParse(request.SignalData, out var signalValue))
        {
             throw new ArgumentException("Invalid signal data: must be an integer");
        }

        await _messageBus.PublishAsync(new SignalReceivedEvent(signalId, signalValue, DateTime.UtcNow), cancellationToken);
        
        _logger.LogInformation("Published SignalReceivedEvent for SignalId: {SignalId}", signalId);

        return signalId;
    }

    // Concrete implementation of the interface for publishing
    private record SignalReceivedEvent(Guid SignalId, int Value, DateTime Timestamp) : SignalReceived;
}
