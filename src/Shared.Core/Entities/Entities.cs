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

// --- Certomo Banking Entities ---

public class CertomoBankDataEntity
{
    public int Id { get; set; }
    public bool Active { get; set; }
    public int? CertomoId { get; set; }
    public int? ParentId { get; set; }
    public DateTime DateTimeOfLastUpdate { get; set; }
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;

    public List<CertomoAccountEntity> Accounts { get; set; } = new();
    public List<CertomoTransactionEntity> Transactions { get; set; } = new();
}

public class CertomoAccountEntity
{
    public int Id { get; set; }
    public int BankDataId { get; set; } // Foreign Key
    public bool Active { get; set; }
    public int? CertomoAccountId { get; set; }
    public int? ParentId { get; set; }
    public string AccountNo { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? AccountBankUniqueId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Bank { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? BankImg { get; set; }
    
    // Additional fields from JSON
    public string? TokenExpirationDate { get; set; }
    public string? TokenExpiresIn { get; set; }
    public string? EquityListJson { get; set; } // Stored as JSON string
    public int CfoAccessInfoId { get; set; }
    public bool? Otp { get; set; }
    public string? LoginType { get; set; }
    public string? LastSuccessfullLoginTime { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? NextDate { get; set; }
    public decimal? OriginalAmount { get; set; }
    public decimal? Rate { get; set; }
}

public class CertomoTransactionEntity
{
    public int Id { get; set; }
    public int BankDataId { get; set; } // Foreign Key
    public bool Active { get; set; }
    public int? CertomoTransactionId { get; set; }
    public int? ParentId { get; set; }
    public string AccountNo { get; set; } = string.Empty;
    public string Bank { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal? BalanceBefore { get; set; }
    public decimal Balance { get; set; }
    public DateTime Date { get; set; }
    public string? BankImg { get; set; }
    public string? Arrow { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? TransCategory { get; set; }
    public string? CategoryColor { get; set; }
    public string? TransClassification { get; set; }
    public string? ClassMatchingStrength { get; set; }
    public bool? IsReport { get; set; }
    public int CfoAccessInfoId { get; set; }
    public DateTime ValueDate { get; set; }
    public string? ObRaw { get; set; } // Stored as JSON string
    public string? BookingStatus { get; set; }
    public string? BankTransactionId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public bool? IsDelta { get; set; }
    public string? ForcastListJson { get; set; } // Stored as JSON string
    public string? CfoName { get; set; }
}
