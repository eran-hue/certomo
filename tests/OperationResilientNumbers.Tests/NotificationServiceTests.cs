using Xunit;
using Moq;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService;
using Shared.Core.Events;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace OperationResilientNumbers.Tests
{
    public class NotificationServiceTests
    {
        [Fact]
        public async Task Consume_AggregationCompleted_ShouldLogNotification()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AggregationCompletedConsumer>>();
            var consumer = new AggregationCompletedConsumer(loggerMock.Object);

            var contextMock = new Mock<ConsumeContext<AggregationCompleted>>();
            var signalId = Guid.NewGuid();
            contextMock.SetupGet(c => c.Message).Returns(Mock.Of<AggregationCompleted>(
                m => m.SignalId == signalId &&
                     m.FinalResult == 100 &&
                     m.Timestamp == DateTime.UtcNow));

            // Act
            await consumer.Consume(contextMock.Object);

            // Assert
            // Verify that logger was called. 
            // Since LogInformation is an extension method, we check for Log call on ILogger
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
