using System.Diagnostics.CodeAnalysis;

using Hare.Builders;
using Hare.Configuration;
using Hare.Contracts;
using Hare.Contracts.Serialization;
using Hare.Contracts.Transport;
using Hare.Infrastructure;
using Hare.Infrastructure.Hosted;
using Hare.Infrastructure.Serialization;
using Hare.Infrastructure.Transport;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;

namespace Hare.Extensions;

/// <summary>
/// Contains extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core services of Hare and returns a builder for further configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Optional configuration for global Hare options.</param>
    public static IHareBuilder AddHare(
        this IServiceCollection services,
        Action<HareOptions>? configure = null
    )
    {
        services.Configure(configure ?? (static _ => { }));
        services.AddSingleton<IEnvelopeSerializer, JsonEnvelopeSerializer>();

        return new HareBuilder(services);
    }

    /// <summary>
    /// Adds a message that Hare can send.
    /// </summary>
    /// <typeparam name="TMessage">The message type Hare should know about.</typeparam>
    public static IServiceCollection AddHareMessage<TMessage>(
        this IServiceCollection services,
        Action<MessageOptions<TMessage>>? configure = null,
        Action<MessageSendOptions<TMessage>>? configureSend = null
    )
    {
        services.Configure(configure ?? (static _ => { }));
        services.Configure(configureSend ?? (static _ => { }));
        services.AddSingleton<IMessageSerializer<TMessage>, JsonMessageSerializer<TMessage>>();
        services.AddSingleton<IMessageSender<TMessage>, RabbitMqMessageSender<TMessage>>();
        services.AddScoped<IMessageProvisioner>(static provider => new DefaultMessageProvisioner<TMessage>(
            provider.GetRequiredService<IOptions<MessageOptions<TMessage>>>(),
            provider.GetRequiredService<IOptions<HareOptions>>(),
            provider.GetService<IOptions<MessageSendOptions<TMessage>>>(),
            provider.GetService<IOptions<MessageReceiveOptions<TMessage>>>(),
            provider.GetRequiredService<IConnection>()
        ));

        return services;
    }

    /// <summary>
    /// Adds a message that Hare can send and receive.
    /// </summary>
    /// <typeparam name="TMessage">The message type Hare should know about.</typeparam>
    /// <typeparam name="THandler">The type that should handle incoming messages of this type.</typeparam>
    public static IServiceCollection AddHareMessage<
        TMessage,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler
    >(
        this IServiceCollection services,
        Action<MessageOptions<TMessage>>? configure = null,
        Action<MessageSendOptions<TMessage>>? configureSend = null,
        Action<MessageReceiveOptions<TMessage>>? configureReceive = null
    ) where THandler : class, IMessageHandler<TMessage>
        => AddHareMessage(services, configure, configureSend)
            .Configure(configureReceive ?? (static _ => { }))
            .AddScoped<IMessageHandler<TMessage>, THandler>()
            .AddHostedService<HostedListenerService<TMessage>>();
}