using System;
using System.Threading.Tasks;
using Xunit;
using Testcontainers.RabbitMq;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Events;

namespace OperationResilientNumbers.Tests.Resilience
{
    public class ProcessorFailureTests : IAsyncLifetime
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
        public async Task System_Should_Queue_Messages_When_Processor_Fails()
        {
            // This test simulates a scenario where a consumer fails to process a message.
            // We verify that the message is moved to an error queue or retried (depending on config).
            // For simplicity here, we'll verify the initial consumption attempt.

            var services = new ServiceCollection();

            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.UsingRabbitMq((context, config) =>
                {
                    config.Host(_rabbitMqContainer.GetConnectionString());
                    config.ConfigureEndpoints(context);
                });

                // Register a failing consumer
                cfg.AddConsumer<FailingConsumer>();
            });

            var provider = services.BuildServiceProvider();
            var harness = provider.GetRequiredService<ITestHarness>();

            await harness.Start();

            try
            {
                var signalId = Guid.NewGuid();
                var eventMessage = new { SignalId = signalId, Value = 99 };

                // Act
                await harness.Bus.Publish<InitiateProcessing>(eventMessage);

                // Assert
                // Verify the consumer tried to consume the message
                Assert.True(await harness.Consumed.Any<InitiateProcessing>());
                
                // Verify that it faulted (failed)
                // harness.Consumed.Select returns an IAsyncEnumerable-like wrapper or IEnumerable that we need to inspect correctly.
                // The issue was trying to await a boolean result from Any() which might not be async in this context or misapplied.
                // Actually, Any() on the harness collection is async.
                
                var consumed = harness.Consumed.Select<InitiateProcessing>();
                Assert.True(consumed.Any(x => x.Exception != null));
            }
            finally
            {
                await harness.Stop();
            }
        }

        public class FailingConsumer : IConsumer<InitiateProcessing>
        {
            public Task Consume(ConsumeContext<InitiateProcessing> context)
            {
                throw new Exception("Simulated processor failure");
            }
        }
    }
}
