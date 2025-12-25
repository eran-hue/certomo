namespace Shared.Core.Entities;

public class Request
{
    public Guid Id { get; set; }
    public int Value { get; set; }
    public DateTime ReceivedAt { get; set; }
}

public class Aggregate
{
    public Guid Id { get; set; } // Matches SignalId
    public int FinalResult { get; set; }
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DataSourceResultEntity> SourceResults { get; set; } = new();
}

public class DataSourceResultEntity
{
    public int Id { get; set; }
    public Guid AggregateId { get; set; }
    public string Source { get; set; } = string.Empty;
    public int Value { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public class FailureLogEntity
{
    public int Id { get; set; }
    public Guid SignalId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}
