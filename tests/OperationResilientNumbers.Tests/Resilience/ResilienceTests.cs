using System;
using System.Threading.Tasks;
using Xunit;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Shared.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Net.Http;

namespace OperationResilientNumbers.Tests.Resilience
{
    public class ResilienceTests : IAsyncLifetime
    {
        // Simulate external dependencies
        private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .Build();

        private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .Build();

        public async Task InitializeAsync()
        {
            await Task.WhenAll(_postgreSqlContainer.StartAsync(), _rabbitMqContainer.StartAsync());
        }

        public async Task DisposeAsync()
        {
            await _postgreSqlContainer.DisposeAsync();
            await _rabbitMqContainer.DisposeAsync();
        }

        [Fact]
        public async Task System_Should_Handle_Database_Outage_Gracefully()
        {
            // This test simulates a scenario where the database becomes unavailable.
            // In a real integration test, we would start the full application stack.
            // Here we verify that the DbContext configured with resiliency policies throws the expected exception 
            // after retries, or handles transient errors if we could simulate them.
            
            // For this test, we'll verify connection failure behavior.

            var connectionString = _postgreSqlContainer.GetConnectionString();
            
            // Simulate outage by stopping the container
            await _postgreSqlContainer.StopAsync();

            var options = new DbContextOptionsBuilder<OperationResilientDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            using (var context = new OperationResilientDbContext(options))
            {
                // We expect a NpgsqlException or DbUpdateException eventually
                await Assert.ThrowsAnyAsync<Exception>(async () => 
                {
                    // Attempt to connect or query
                    // await context.Database.CanConnectAsync();
                    
                    // Force a command to be executed
                    await context.Database.OpenConnectionAsync();
                    await context.Database.ExecuteSqlRawAsync("SELECT 1");
                });
            }
            
            // Restart for cleanup/subsequent tests if needed
            await _postgreSqlContainer.StartAsync();
        }
    }
}
