namespace Shared.Infrastructure.Extensions;

using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.Messaging;
using Shared.Application.Abstractions;
using Shared.Infrastructure.Configuration;
using System;

public static class MassTransitExtensions
{
    public static void AddSharedMassTransit(this IServiceCollection services, IConfiguration configuration, Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        var rabbitMqOptions = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() ?? new RabbitMqOptions();

        services.AddMassTransit(x =>
        {
            configureConsumers?.Invoke(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqOptions.Host, "/", h =>
                {
                    h.Username(rabbitMqOptions.Username);
                    h.Password(rabbitMqOptions.Password);
                });

                // Observability: Propagate CorrelationId
                cfg.UseConsumeFilter(typeof(CorrelationIdConsumeFilter<>), context);

                // Resilience: Retry Policy
                cfg.UseMessageRetry(r => r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)));

                // Resilience: Dead Letter Queue (DLQ) is enabled by default in RabbitMQ transport
                
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IMessageBus, MassTransitMessageBus>();
    }
}

public class CorrelationIdConsumeFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    public void Probe(ProbeContext context) { }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        // CorrelationId is automatically propagated by MassTransit.
        // We can add it to the logging context here if needed, but Serilog's FromLogContext 
        // combined with MassTransit's existing logging integration usually handles this.
        // However, explicitly ensuring it for other loggers is good practice.
        
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", context.CorrelationId))
        {
            await next.Send(context);
        }
    }
}
