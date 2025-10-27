using Hare.Configuration;
using Hare.Configuration.Validation;
using Hare.Contracts;
using Hare.Hosted;
using Hare.Services;

using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contains extension methods for <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the base services and types to make Hare work.
    /// Also handles configuring the telemetry for tracing.
    /// </summary>
    public static IServiceCollection AddHare(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddSource("Hare"));

        return services;
    }

    /// <summary>
    /// Registers a message that Hare should know how to handle.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to register the services into.</param>
    /// <param name="configure">An optional configuration callback.</param>
    /// <param name="listen">If set to <c>true</c> will also register a listener.</param>
    /// <typeparam name="TMessage">The message type that should be handled.</typeparam>
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