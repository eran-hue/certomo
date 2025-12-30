using System.Text.Json.Serialization;

namespace Shared.Application.DTOs;

// --- Authentication ---
public record CertomoAuthRequest(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("Password")] string Password
);

public record CertomoAuthResponse(
    [property: JsonPropertyName("jwt")] string Jwt
);

// --- Banks ---
public record CertomoBank(
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("bankImg")] string BankImg,
    [property: JsonPropertyName("baseCurrency")] string BaseCurrency
);

// --- Bank Data (Accounts & Transactions) ---
public record CertomoBankDataResponse(
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("id")] int? Id,
    [property: JsonPropertyName("parentId")] int? ParentId,
    [property: JsonPropertyName("accountsList")] List<CertomoAccount> AccountsList,
    [property: JsonPropertyName("transactionList")] List<CertomoTransaction> TransactionList,
    [property: JsonPropertyName("dateTimeOfLastUpdate")] string DateTimeOfLastUpdate
);

public record CertomoAccount(
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("id")] int? Id,
    [property: JsonPropertyName("parentId")] int? ParentId,
    [property: JsonPropertyName("account_no")] string AccountNo,
    [property: JsonPropertyName("accountName")] string AccountName,
    [property: JsonPropertyName("accountBankUniqueId")] string AccountBankUniqueId,
    [property: JsonPropertyName("entityName")] string EntityName,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("amount")] string Amount, // API returns string "55555551.27"
    [property: JsonPropertyName("bank")] string Bank,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("bank_img")] string BankImg,
    [property: JsonPropertyName("token_expiration_date")] string? TokenExpirationDate,
    [property: JsonPropertyName("tokenExpiresIn")] string? TokenExpiresIn,
    [property: JsonPropertyName("equityList")] object? EquityList, // Using object for complex/unknown structure
    [property: JsonPropertyName("cfoAccessInfoId")] int CfoAccessInfoId,
    [property: JsonPropertyName("otp")] bool? Otp,
    [property: JsonPropertyName("loginType")] string? LoginType,
    [property: JsonPropertyName("lastSuccessfullLoginTime")] string? LastSuccessfullLoginTime,
    [property: JsonPropertyName("startDate")] string? StartDate,
    [property: JsonPropertyName("endDate")] string? EndDate,
    [property: JsonPropertyName("nextDate")] string? NextDate,
    [property: JsonPropertyName("originalAmount")] string? OriginalAmount,
    [property: JsonPropertyName("rate")] string? Rate
);

public record CertomoTransaction(
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("id")] int? Id,
    [property: JsonPropertyName("parentId")] int? ParentId,
    [property: JsonPropertyName("account_no")] string AccountNo,
    [property: JsonPropertyName("bank")] string Bank,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("amount")] string Amount,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("balance_before")] string? BalanceBefore,
    [property: JsonPropertyName("balance")] string Balance,
    [property: JsonPropertyName("date")] string Date,
    [property: JsonPropertyName("bank_img")] string BankImg,
    [property: JsonPropertyName("arrow")] string? Arrow,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("transCategory")] string? TransCategory,
    [property: JsonPropertyName("categoryColor")] string? CategoryColor,
    [property: JsonPropertyName("transClassification")] string? TransClassification,
    [property: JsonPropertyName("classMatchingStrength")] string? ClassMatchingStrength,
    [property: JsonPropertyName("isReport")] bool? IsReport,
    [property: JsonPropertyName("cfoAccessInfoId")] int CfoAccessInfoId,
    [property: JsonPropertyName("valueDate")] string ValueDate,
    [property: JsonPropertyName("obRaw")] string? ObRaw,
    [property: JsonPropertyName("bookingStatus")] string? BookingStatus,
    [property: JsonPropertyName("bankTransactionId")] string? BankTransactionId,
    [property: JsonPropertyName("reference")] string Reference,
    [property: JsonPropertyName("entity")] string Entity,
    [property: JsonPropertyName("isDelta")] bool? IsDelta,
    [property: JsonPropertyName("forcastList")] object? ForcastList, // Using object for complex/unknown structure
    [property: JsonPropertyName("cfoName")] string? CfoName
);

// --- Israel Open Banking Auth ---
public record CertomoIsraelAuthRequest(
    string PsuId,
    string? BusinessUid,
    int BankId,
    string AccountType,
    bool TsAndCs,
    string Partner,
    string Channel = "API"
);

public record CertomoIsraelAuthResponse(
    [property: JsonPropertyName("authUrl")] string AuthUrl,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("accessInfoId")] int AccessInfoId,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("timestamp")] string Timestamp
);
