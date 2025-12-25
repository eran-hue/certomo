namespace Shared.Core.Events;

public interface SignalReceived
{
    Guid SignalId { get; }
    int Value { get; }
    DateTime Timestamp { get; }
}

public interface InitiateProcessing
{
    Guid SignalId { get; }
    int Value { get; }
    DateTime Timestamp { get; }
}

public interface DataProcessed
{
    Guid SignalId { get; }
    string ProcessorName { get; }
    int ProcessedValue { get; }
    DateTime Timestamp { get; }
}

public interface AggregationCompleted
{
    Guid SignalId { get; }
    int FinalResult { get; }
    DateTime Timestamp { get; }
}

public interface ProcessFailed
{
    Guid SignalId { get; }
    string Reason { get; }
    string ServiceName { get; }
    DateTime Timestamp { get; }
}
