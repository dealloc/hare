using System.Diagnostics.CodeAnalysis;

using Hare.Configuration;
using Hare.Contracts;
using Hare.Contracts.Serialization;
using Hare.Infrastructure.Serialization;
using Hare.Infrastructure.Transport;

using Microsoft.Extensions.DependencyInjection;

namespace Hare.Extensions;

/// <summary>
/// Contains extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core services of Hare.
    /// </summary>
    public static IServiceCollection AddHare(this IServiceCollection services)
    {
        services.Configure<HareOptions>(static _ => { });
        services.AddSingleton<IEnvelopeSerializer, JsonEnvelopeSerializer>();

        return services;
    }

    /// <summary>
    /// Adds a message that Hare can send.
    /// </summary>
    /// <typeparam name="TMessage">The message type Hare should know about.</typeparam>
    public static IServiceCollection AddHareMessage<TMessage>(this IServiceCollection services)
    {
        services.Configure<MessageOptions<TMessage>>(static _ => { });
        services.AddSingleton<IMessageSerializer<TMessage>, JsonMessageSerializer<TMessage>>();

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
        this IServiceCollection services
    ) where THandler : class, IMessageHandler<TMessage>
        => AddHareMessage<TMessage>(services)
            .AddScoped<IMessageHandler<TMessage>, THandler>()
            .AddHostedService<HostedListenerService<TMessage>>();
}