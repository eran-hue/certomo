namespace Shared.Infrastructure.Logging;

using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

public static class SerilogConfiguration
{
    public static void ConfigureLogging(string serviceName)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", serviceName)
            .WriteTo.Console()
            .WriteTo.Seq("http://seq:5341") // Assuming Seq is running in Docker as 'seq'
            .CreateLogger();
    }
}
