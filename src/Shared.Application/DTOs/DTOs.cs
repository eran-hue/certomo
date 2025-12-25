namespace Shared.Application.DTOs;

public record SignalRequest(int Value);

public record DataSourceResult(string Source, int Value);

public record FailureLog(Guid SignalId, string Reason, DateTime OccurredAt);
