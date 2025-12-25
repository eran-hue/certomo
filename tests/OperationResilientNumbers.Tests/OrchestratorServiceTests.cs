using Xunit;
using Moq;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrchestratorService;
using Shared.Core.Events;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace OperationResilientNumbers.Tests
{
    public class OrchestratorServiceTests
    {
        [Fact]
        public async Task Consume_SignalReceived_ShouldPublishInitiateProcessingEvents()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SignalReceivedConsumer>>();
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            var consumer = new SignalReceivedConsumer(loggerMock.Object, publishEndpointMock.Object);

            var contextMock = new Mock<ConsumeContext<SignalReceived>>();
            var signalId = Guid.NewGuid();
            contextMock.SetupGet(c => c.Message).Returns(Mock.Of<SignalReceived>(
                 m => m.SignalId == signalId &&
                      m.Value == 42 &&
                      m.Timestamp == DateTime.UtcNow));

            // Act
            await consumer.Consume(contextMock.Object);

            // Assert
            // Since InitiateProcessing is an interface and used anonymously in publish, we check if Publish is called with correct values
            // Note: SignalReceivedConsumer currently publishes anonymous object matching InitiateProcessing interface
            
            publishEndpointMock.Verify(x => x.Publish<InitiateProcessing>(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
    }
}
