using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.Configuration;
using Shared.Application.Abstractions;
using Shared.Infrastructure.Services;
using Polly;
using Polly.Extensions.Http;

namespace Shared.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static void AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<OperationResilientDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString(DatabaseOptions.ConnectionStringName)));

        // Certomo Service Configuration
        services.Configure<CertomoOptions>(configuration.GetSection(CertomoOptions.SectionName));

        // HttpClient with Resilience
        services.AddHttpClient<ICertomoService, CertomoService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CertomoOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        })
        .AddPolicyHandler(GetRetryPolicy());
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
