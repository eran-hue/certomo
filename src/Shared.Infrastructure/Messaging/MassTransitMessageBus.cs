namespace Shared.Infrastructure.Messaging;

using MassTransit;
using Shared.Application.Abstractions;
using System.Threading;
using System.Threading.Tasks;

public class MassTransitMessageBus : IMessageBus
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    public MassTransitMessageBus(IPublishEndpoint publishEndpoint, ISendEndpointProvider sendEndpointProvider)
    {
        _publishEndpoint = publishEndpoint;
        _sendEndpointProvider = sendEndpointProvider;
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        await _publishEndpoint.Publish(message, cancellationToken);
    }

    public async Task SendAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        // MassTransit Send requires a specific endpoint address usually, but SendEndpointProvider can be used if we know the destination.
        // For general "Send" where we might rely on topology or other mechanisms, typically we need a destination.
        // However, if the interface implies just "Send", it's a bit ambiguous without an address in MassTransit.
        // Assuming for now we might not use SendAsync often or we will need to address the destination issue.
        // But to satisfy the interface, let's see. 
        // If the intent is just to "Send" to a queue determined by the message type (which MassTransit does for commands if configured), 
        // we might need to look up the endpoint.
        // But for now, to fix the build, I will throw NotImplementedException or try to use _sendEndpointProvider if I had an address.
        // A better approach for now might be to inject specific send endpoints or just use Publish for everything if that's the pattern.
        // But the interface has SendAsync. 
        // Let's implement it by getting the send endpoint for the message type if possible, or throwing.
        // Actually, without a destination address, Send is hard in MassTransit unless we have a convention.
        // Let's throw NotImplementedException for now as we are focusing on Publish in the handler.
        
        throw new NotImplementedException("SendAsync is not yet implemented without a destination address.");
    }
}
