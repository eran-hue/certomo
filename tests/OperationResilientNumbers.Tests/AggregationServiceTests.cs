using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MassTransit;
using AggregationService;
using Shared.Core.Events;
using Shared.Infrastructure.Data;
using Shared.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OperationResilientNumbers.Tests
{
    public class AggregationServiceTests
    {
        [Fact]
        public async Task Consume_DataProcessed_ShouldAggreagteResults()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<OperationResilientDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using var dbContext = new OperationResilientDbContext(options);
            var loggerMock = new Mock<ILogger<DataProcessedConsumer>>();
            var publishEndpointMock = new Mock<IPublishEndpoint>();

            var consumer = new DataProcessedConsumer(loggerMock.Object, publishEndpointMock.Object, dbContext);

            var signalId = Guid.NewGuid();
            var contextMock = new Mock<ConsumeContext<DataProcessed>>();
            contextMock.SetupGet(c => c.Message).Returns(Mock.Of<DataProcessed>(
                m => m.SignalId == signalId &&
                     m.ProcessorName == "Processor-1" &&
                     m.ProcessedValue == 10 &&
                     m.Timestamp == DateTime.UtcNow));

            // Act
            await consumer.Consume(contextMock.Object);

            // Assert
            var aggregate = await dbContext.Aggregates.Include(a => a.SourceResults).FirstOrDefaultAsync(a => a.Id == signalId);
            Assert.NotNull(aggregate);
            Assert.Single(aggregate.SourceResults);
            Assert.Equal(10, aggregate.SourceResults.First().Value);
        }

        [Fact]
        public async Task Consume_DuplicateDataProcessed_ShouldBeIdempotent()
        {
            // Arrange
             var options = new DbContextOptionsBuilder<OperationResilientDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Idempotency")
                .Options;

            using var dbContext = new OperationResilientDbContext(options);
            var loggerMock = new Mock<ILogger<DataProcessedConsumer>>();
            var publishEndpointMock = new Mock<IPublishEndpoint>();

            var consumer = new DataProcessedConsumer(loggerMock.Object, publishEndpointMock.Object, dbContext);
            var signalId = Guid.NewGuid();

            var contextMock = new Mock<ConsumeContext<DataProcessed>>();
            contextMock.SetupGet(c => c.Message).Returns(Mock.Of<DataProcessed>(
                m => m.SignalId == signalId &&
                     m.ProcessorName == "Processor-1" &&
                     m.ProcessedValue == 10 &&
                     m.Timestamp == DateTime.UtcNow));

            // Act
            await consumer.Consume(contextMock.Object); // First processing
            await consumer.Consume(contextMock.Object); // Duplicate processing

            // Assert
            var aggregate = await dbContext.Aggregates.Include(a => a.SourceResults).FirstOrDefaultAsync(a => a.Id == signalId);
            Assert.NotNull(aggregate);
            Assert.Single(aggregate.SourceResults); // Should still be 1
        }
    }
}
