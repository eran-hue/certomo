using Microsoft.EntityFrameworkCore;
using Shared.Core.Events;
using Shared.Infrastructure.Data;
using MassTransit;

namespace AggregationService;

public class Worker(ILogger<Worker> logger, IServiceProvider serviceProvider) : BackgroundService
{
    private const int TIMEOUT_SECONDS = 30; // Timeout after 30 seconds
    private const int EXPECTED_RESULTS_COUNT = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try 
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<OperationResilientDbContext>();
                    var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                    var now = DateTime.UtcNow;
                    var timeoutThreshold = now.AddSeconds(-TIMEOUT_SECONDS);

                    // Find incomplete aggregates that have timed out
                    var timedOutAggregates = await dbContext.Aggregates
                        .Include(a => a.SourceResults)
                        .Where(a => !a.IsComplete && a.CreatedAt < timeoutThreshold)
                        .ToListAsync(stoppingToken);

                    foreach (var aggregate in timedOutAggregates)
                    {
                        logger.LogWarning("Aggregation Timeout for Signal {SignalId}. Processed {Count}/{Expected} results.", aggregate.Id, aggregate.SourceResults.Count, EXPECTED_RESULTS_COUNT);

                        aggregate.IsComplete = true;
                        aggregate.FinalResult = aggregate.SourceResults.Sum(x => x.Value); // Partial sum

                        await publishEndpoint.Publish<AggregationCompleted>(new 
                        {
                            SignalId = aggregate.Id,
                            FinalResult = aggregate.FinalResult,
                            Timestamp = DateTime.UtcNow
                        }, stoppingToken);

                        logger.LogInformation("Partial Aggregation Completed (Timeout): {SignalId}. Final Result: {FinalResult}", aggregate.Id, aggregate.FinalResult);
                    }

                    if (timedOutAggregates.Any())
                    {
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Aggregation Timeout Worker");
            }

            await Task.Delay(5000, stoppingToken); // Check every 5 seconds
        }
    }
}
