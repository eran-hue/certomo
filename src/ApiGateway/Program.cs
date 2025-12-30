using Microsoft.AspNetCore.Mvc;
using Shared.Application.DTOs;
using MediatR;
using ApiGateway.Features.Signals.SubmitSignal;
using Shared.Application.Behaviors;
using ApiGateway.Middleware;
using Shared.Infrastructure.Logging;
using Serilog;
using Shared.Infrastructure.Extensions;
using FluentValidation;
using Scalar.AspNetCore;

SerilogConfiguration.ConfigureLogging("ApiGateway");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddOpenApi();

// Register FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Register MediatR
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Register Shared Infrastructure
builder.Services.AddSharedInfrastructure(builder.Configuration);

// Configure MassTransit using Shared Extension
builder.Services.AddSharedMassTransit(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler(); // This must be first
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Serve static files (HTML/CSS/JS)

app.UseSerilogRequestLogging();

// Implement endpoint to accept signals
app.MapPost("/api/signals", async ([FromBody] SignalRequest request, IMediator mediator) =>
{
    var command = new SubmitSignalCommand(request.Value.ToString());
    var signalId = await mediator.Send(command);
    return Results.Accepted(value: new { SignalId = signalId });
})
.WithName("SubmitSignal");

// Endpoint to fetch Certomo Data
app.MapGet("/api/certomo-data", async (
    [FromServices] Shared.Application.Abstractions.ICertomoService certomoService, 
    [FromServices] Shared.Infrastructure.Data.OperationResilientDbContext dbContext,
    [FromServices] Microsoft.Extensions.Configuration.IConfiguration configuration) =>
{
    try
    {
        var username = configuration["Certomo:Username"];
        var password = configuration["Certomo:Password"];
        var secret = configuration["Certomo:Secret"];

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(secret))
        {
            return Results.Problem("Missing Certomo credentials.");
        }

        var authResponse = await certomoService.AuthenticateAsync(username, password);
        var bankData = await certomoService.GetBankDataAsync(authResponse.Jwt, username, secret);

        // --- Save to Database ---
        try
        {
            // Map DTO to Entity
            var bankDataEntity = new Shared.Core.Entities.CertomoBankDataEntity
            {
                Active = bankData.Active,
                CertomoId = bankData.Id,
                ParentId = bankData.ParentId,
                DateTimeOfLastUpdate = DateTime.TryParse(bankData.DateTimeOfLastUpdate, out var dt) ? dt : DateTime.UtcNow,
                Accounts = bankData.AccountsList.Select(a => new Shared.Core.Entities.CertomoAccountEntity
                {
                    Active = a.Active,
                    CertomoAccountId = a.Id,
                    ParentId = a.ParentId,
                    AccountNo = a.AccountNo,
                    AccountName = a.AccountName,
                    AccountBankUniqueId = a.AccountBankUniqueId,
                    EntityName = a.EntityName,
                    Currency = a.Currency,
                    Amount = decimal.TryParse(a.Amount, out var amt) ? amt : 0,
                    Bank = a.Bank,
                    Type = a.Type,
                    BankImg = a.BankImg,
                    TokenExpirationDate = a.TokenExpirationDate,
                    TokenExpiresIn = a.TokenExpiresIn,
                    EquityListJson = a.EquityList != null ? System.Text.Json.JsonSerializer.Serialize(a.EquityList) : null,
                    CfoAccessInfoId = a.CfoAccessInfoId,
                    Otp = a.Otp,
                    LoginType = a.LoginType,
                    LastSuccessfullLoginTime = a.LastSuccessfullLoginTime,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    NextDate = a.NextDate,
                    OriginalAmount = decimal.TryParse(a.OriginalAmount, out var origAmt) ? origAmt : null,
                    Rate = decimal.TryParse(a.Rate, out var rate) ? rate : null
                }).ToList(),
                Transactions = bankData.TransactionList.Select(t => new Shared.Core.Entities.CertomoTransactionEntity
                {
                    Active = t.Active,
                    CertomoTransactionId = t.Id,
                    ParentId = t.ParentId,
                    AccountNo = t.AccountNo,
                    Bank = t.Bank,
                    Type = t.Type,
                    Amount = decimal.TryParse(t.Amount, out var tAmt) ? tAmt : 0,
                    Currency = t.Currency,
                    BalanceBefore = decimal.TryParse(t.BalanceBefore, out var bb) ? bb : null,
                    Balance = decimal.TryParse(t.Balance, out var b) ? b : 0,
                    Date = DateTime.TryParse(t.Date, out var date) ? date : DateTime.UtcNow,
                    BankImg = t.BankImg,
                    Arrow = t.Arrow,
                    Description = t.Description,
                    TransCategory = t.TransCategory,
                    CategoryColor = t.CategoryColor,
                    TransClassification = t.TransClassification,
                    ClassMatchingStrength = t.ClassMatchingStrength,
                    IsReport = t.IsReport,
                    CfoAccessInfoId = t.CfoAccessInfoId,
                    ValueDate = DateTime.TryParse(t.ValueDate, out var vd) ? vd : DateTime.UtcNow,
                    ObRaw = t.ObRaw,
                    BookingStatus = t.BookingStatus,
                    BankTransactionId = t.BankTransactionId,
                    Reference = t.Reference,
                    Entity = t.Entity,
                    IsDelta = t.IsDelta,
                    ForcastListJson = t.ForcastList != null ? System.Text.Json.JsonSerializer.Serialize(t.ForcastList) : null,
                    CfoName = t.CfoName
                }).ToList()
            };

            dbContext.CertomoBankData.Add(bankDataEntity);
            await dbContext.SaveChangesAsync();
            Log.Information("Successfully saved Certomo data to database with ID: {Id}", bankDataEntity.Id);
        }
        catch (Exception dbEx)
        {
            Log.Error(dbEx, "Failed to save Certomo data to database, but returning data to frontend.");
            // We do not re-throw here so the frontend still gets the data even if DB save fails
        }
        // ------------------------

        return Results.Ok(bankData);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error fetching Certomo data");
        return Results.Problem("Failed to fetch data from Certomo.");
    }
})
.WithName("GetCertomoData");

try
{
    // Ensure database is created (for dev environments without migrations)
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<Shared.Infrastructure.Data.OperationResilientDbContext>();
        db.Database.EnsureCreated();
    }

    Log.Information("Starting ApiGateway");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ApiGateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

