using MassTransit;
using Shared.Core.Events;
using Shared.Infrastructure.Data;
using Shared.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AggregationService;

public class DataProcessedConsumer : IConsumer<DataProcessed>
{
    private readonly ILogger<DataProcessedConsumer> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly OperationResilientDbContext _dbContext;
    private const int EXPECTED_RESULTS_COUNT = 3; // Now expecting 3 results

    public DataProcessedConsumer(ILogger<DataProcessedConsumer> logger, IPublishEndpoint publishEndpoint, OperationResilientDbContext dbContext)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<DataProcessed> context)
    {
        _logger.LogInformation("Data Processed Received: {SignalId} from {Processor}", context.Message.SignalId, context.Message.ProcessorName);

        // Find or create the Aggregate
        var aggregate = await _dbContext.Aggregates
            .Include(a => a.SourceResults)
            .FirstOrDefaultAsync(a => a.Id == context.Message.SignalId);

        if (aggregate == null)
        {
            aggregate = new Aggregate
            {
                Id = context.Message.SignalId,
                CreatedAt = DateTime.UtcNow,
                IsComplete = false
            };
            _dbContext.Aggregates.Add(aggregate);
        }

        // Check if we already have this result (Idempotency)
        // Check in memory first
        if (aggregate.SourceResults.Any(r => r.Source == context.Message.ProcessorName))
        {
            _logger.LogInformation("Duplicate result received for Signal {SignalId} from {Processor}", context.Message.SignalId, context.Message.ProcessorName);
            return;
        }

        try 
        {
            // Add the new result
            aggregate.SourceResults.Add(new DataSourceResultEntity
            {
                AggregateId = aggregate.Id,
                Source = context.Message.ProcessorName,
                Value = context.Message.ProcessedValue,
                ProcessedAt = context.Message.Timestamp
            });

            // Determine if aggregation is complete
            bool isComplete = aggregate.SourceResults.Count >= EXPECTED_RESULTS_COUNT; 

            if (isComplete && !aggregate.IsComplete)
            {
                aggregate.IsComplete = true;
                aggregate.FinalResult = aggregate.SourceResults.Sum(x => x.Value); // Example aggregation: Sum

                await _publishEndpoint.Publish<AggregationCompleted>(new 
                {
                    SignalId = aggregate.Id,
                    FinalResult = aggregate.FinalResult,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("Aggregation Completed: {SignalId}. Final Result: {FinalResult}", aggregate.Id, aggregate.FinalResult);
            }

            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
             // Handle unique constraint violation (concurrency/race condition)
            if (ex.InnerException != null && ex.InnerException.Message.Contains("unique constraint"))
            {
                 _logger.LogInformation("Duplicate result detected during save for Signal {SignalId} from {Processor}", context.Message.SignalId, context.Message.ProcessorName);
                 return; // Treat as success/idempotent
            }
            throw;
        }
    }
}
