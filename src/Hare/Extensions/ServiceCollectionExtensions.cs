using Hare.Configuration;
using Hare.Configuration.Validation;
using Hare.Contracts;
using Hare.Hosted;
using Hare.Services;

using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHare(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddSource("Hare"));

        return services;
    }

    public static IServiceCollection AddHareMessage<TMessage>(
        this IServiceCollection services,
        Action<HareMessageConfiguration<TMessage>>? configure = null,
        bool listen = false
    ) where TMessage : class
    {
        services.AddOptions<HareMessageConfiguration<TMessage>>();
        services
            .AddSingleton<IValidateOptions<HareMessageConfiguration<TMessage>>,
                HareMessageConfigurationValidator<TMessage>>();

        if (configure is not null)
            services.Configure(configure);

        if (listen)
            services.AddHostedService<MessageReceiverService<TMessage>>();

        services.AddScoped<IMessageSender<TMessage>, DefaultMessageSender<TMessage>>();
        return services;
    }
}