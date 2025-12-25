using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Entities;
using Shared.Infrastructure.Data;
using Testcontainers.PostgreSql;
using Xunit;

namespace OperationResilientNumbers.Tests.Integration
{
    public class DatabasePersistenceTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .Build();

        public async Task InitializeAsync()
        {
            await _postgreSqlContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _postgreSqlContainer.DisposeAsync();
        }

        [Fact]
        public async Task Can_Save_And_Retrieve_Aggregate()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<OperationResilientDbContext>()
                .UseNpgsql(_postgreSqlContainer.GetConnectionString())
                .Options;

            using (var context = new OperationResilientDbContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                var aggregate = new Aggregate
                {
                    Id = System.Guid.NewGuid(),
                    CreatedAt = System.DateTime.UtcNow,
                    IsComplete = false,
                    FinalResult = 0
                };

                // Act
                context.Aggregates.Add(aggregate);
                await context.SaveChangesAsync();
            }

            // Assert
            using (var context = new OperationResilientDbContext(options))
            {
                var count = await context.Aggregates.CountAsync();
                Assert.Equal(1, count);

                var savedAggregate = await context.Aggregates.FirstOrDefaultAsync();
                Assert.NotNull(savedAggregate);
            }
        }

    }
}
