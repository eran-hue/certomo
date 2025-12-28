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
    [property: JsonPropertyName("entityName")] string EntityName,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("amount")] string Amount, // API returns string "55555551.27"
    [property: JsonPropertyName("bank")] string Bank,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("bank_img")] string BankImg,
    [property: JsonPropertyName("cfoAccessInfoId")] int CfoAccessInfoId
);

public record CertomoTransaction(
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("id")] int? Id,
    [property: JsonPropertyName("account_no")] string AccountNo,
    [property: JsonPropertyName("bank")] string Bank,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("amount")] string Amount,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("balance")] string Balance,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("valueDate")] string ValueDate,
    [property: JsonPropertyName("reference")] string Reference,
    [property: JsonPropertyName("entity")] string Entity
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
