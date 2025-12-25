using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Events;
using OrchestratorService;
using Testcontainers.RabbitMq;
using Xunit;

namespace OperationResilientNumbers.Tests.Integration
{
    public class RabbitMqMessagingTests : IAsyncLifetime
    {
        private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .Build();

        public async Task InitializeAsync()
        {
            await _rabbitMqContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _rabbitMqContainer.DisposeAsync();
        }

        [Fact]
        public async Task Can_Publish_And_Consume_Message()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddLogging();

            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<SignalReceivedConsumer>();

                cfg.UsingRabbitMq((context, config) =>
                {
                    config.Host(_rabbitMqContainer.GetConnectionString());
                    config.ConfigureEndpoints(context);
                });
            });

            var provider = services.BuildServiceProvider();
            var harness = provider.GetRequiredService<ITestHarness>();

            await harness.Start();

            try
            {
                var signalId = Guid.NewGuid();
                var eventMessage = new { SignalId = signalId, Value = 42 };

                // Act
                await harness.Bus.Publish<SignalReceived>(eventMessage);

                // Assert
                Assert.True(await harness.Consumed.Any<SignalReceived>());
                Assert.True(await harness.Published.Any<InitiateProcessing>());
                
                var publishedMessage = harness.Published.Select<InitiateProcessing>().FirstOrDefault();
                Assert.NotNull(publishedMessage);
                Assert.Equal(signalId, publishedMessage.Context.Message.SignalId);
            }
            finally
            {
                await harness.Stop();
            }
        }
    }
}
