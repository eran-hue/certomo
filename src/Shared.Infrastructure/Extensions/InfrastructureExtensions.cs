namespace Shared.Infrastructure.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.Configuration;

public static class InfrastructureExtensions
{
    public static void AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<OperationResilientDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString(DatabaseOptions.ConnectionStringName)));

        // Add other shared infrastructure services here if any
    }
}
