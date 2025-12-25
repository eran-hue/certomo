using Xunit;
using Moq;
using MassTransit;
using Microsoft.Extensions.Logging;
using DataProcessorService;
using Shared.Core.Events;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace OperationResilientNumbers.Tests
{
    public class DataProcessorServiceTests
    {
        [Fact]
        public async Task Consume_InitiateProcessing_ShouldProcessAndPublishEvent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<InitiateProcessingConsumer>>();
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            var consumer = new InitiateProcessingConsumer(loggerMock.Object, publishEndpointMock.Object);

            var contextMock = new Mock<ConsumeContext<InitiateProcessing>>();
            var signalId = Guid.NewGuid();
            contextMock.SetupGet(c => c.Message).Returns(Mock.Of<InitiateProcessing>(
                m => m.SignalId == signalId &&
                     m.Value == 10 &&
                     m.Timestamp == DateTime.UtcNow));

            // Set environment variable for processor name
            Environment.SetEnvironmentVariable("PROCESSOR_NAME", "TestProcessor");

            // Act
            await consumer.Consume(contextMock.Object);

            // Assert
            // Since DataProcessed is an interface and published anonymously, we check for Publish call with compatible object
            publishEndpointMock.Verify(x => x.Publish<DataProcessed>(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
