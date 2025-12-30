using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions;
using Shared.Infrastructure.Configuration;
using Shared.Infrastructure.Extensions;

namespace CertomoDataFetcher;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("src/CertomoDataFetcher/appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                // Register Shared Infrastructure
                services.AddSharedInfrastructure(hostContext.Configuration);
                
                // Add the worker
                services.AddHostedService<DataFetcherWorker>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .Build();

        await host.RunAsync();
    }
}

public class DataFetcherWorker : BackgroundService
{
    private readonly ICertomoService _certomoService;
    private readonly ILogger<DataFetcherWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHostApplicationLifetime _lifetime;

    public DataFetcherWorker(
        ICertomoService certomoService, 
        ILogger<DataFetcherWorker> logger, 
        IConfiguration configuration,
        IHostApplicationLifetime lifetime)
    {
        _certomoService = certomoService;
        _logger = logger;
        _configuration = configuration;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting Certomo Data Fetcher...");

            // 1. Get Credentials
            var username = _configuration["Certomo:Username"];
            var password = _configuration["Certomo:Password"];
            var secret = _configuration["Certomo:Secret"];

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(secret))
            {
                _logger.LogError("Missing Certomo credentials in configuration (Username, Password, Secret).");
                return;
            }

            // 2. Authenticate
            _logger.LogInformation("Authenticating user: {Username}", username);
            var authResponse = await _certomoService.AuthenticateAsync(username, password, stoppingToken);
            _logger.LogInformation("Authentication successful. Token received.");

            // 3. Fetch Bank Data
            _logger.LogInformation("Fetching bank data...");
            var bankData = await _certomoService.GetBankDataAsync(authResponse.Jwt, username, secret, stoppingToken);

            // 4. Log Results
            _logger.LogInformation("Bank Data Fetched Successfully!");
            _logger.LogInformation("Last Update: {LastUpdate}", bankData.DateTimeOfLastUpdate);
            _logger.LogInformation("Accounts Found: {Count}", bankData.AccountsList?.Count ?? 0);
            
            if (bankData.AccountsList != null)
            {
                foreach (var account in bankData.AccountsList)
                {
                    _logger.LogInformation(" - Account: {AccountName} ({AccountNo}) - Balance: {Amount} {Currency} - Bank: {Bank}", 
                        account.AccountName, account.AccountNo, account.Amount, account.Currency, account.Bank);
                }
            }

            _logger.LogInformation("Transactions Found: {Count}", bankData.TransactionList?.Count ?? 0);
             if (bankData.TransactionList != null)
            {
                foreach (var tx in bankData.TransactionList.Take(5)) // Show first 5
                {
                    _logger.LogInformation(" - Transaction: {Description} - {Amount} {Currency} - Date: {Date}", 
                        tx.Description, tx.Amount, tx.Currency, tx.ValueDate);
                }
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching Certomo data.");
        }
        finally
        {
             _lifetime.StopApplication();
        }
    }
}
