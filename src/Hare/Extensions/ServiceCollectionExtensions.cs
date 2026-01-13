using System.Diagnostics.CodeAnalysis;

using Hare.Configuration;
using Hare.Contracts;
using Hare.Contracts.Serialization;
using Hare.Infrastructure.Serialization;

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
    public static IServiceCollection AddKeyedHare(this IServiceCollection services, string? key = null)
    {
        services.Configure<HareOptions>(key, static _ => { });
        services.AddKeyedSingleton<IEnvelopeSerializer, JsonEnvelopeSerializer>(key);

        return services;
    }

    /// <inheritdoc cref="AddKeyedHare(IServiceCollection, string?)" />
    public static IServiceCollection AddHare(this IServiceCollection services)
        => AddKeyedHare(services, key: null);

    /// <summary>
    /// Adds a message that Hare can send.
    /// </summary>
    /// <typeparam name="TMessage">The message type Hare should know about.</typeparam>
    public static IServiceCollection AddKeyedHareMessage<TMessage>(this IServiceCollection services, string? key = null)
    {
        services.Configure<MessageOptions<TMessage>>(key, static _ => { });
        services.AddKeyedSingleton<IMessageSerializer<TMessage>, JsonMessageSerializer<TMessage>>(key);

        return services;
    }

    /// <inheritdoc cref="AddKeyedHareMessage{TMessage}(IServiceCollection, string?)" />
    public static IServiceCollection AddHareMessage<TMessage>(this IServiceCollection services)
        => AddKeyedHareMessage<TMessage>(services, key: null);

    /// <summary>
    /// Adds a message that Hare can send and receive.
    /// </summary>
    /// <typeparam name="TMessage">The message type Hare should know about.</typeparam>
    /// <typeparam name="THandler">The type that should handle incoming messages of this type.</typeparam>
    public static IServiceCollection AddKeyedHareMessage<
        TMessage,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler
    >(
        this IServiceCollection services,
        string? key = null
    ) where THandler : class, IMessageHandler<TMessage>
        => AddKeyedHareMessage<TMessage>(services, key)
            .AddKeyedScoped<IMessageHandler<TMessage>, THandler>(key);

    /// <inheritdoc cref="AddKeyedHareMessage{TMessage, THandler}(IServiceCollection, string?)" />
    public static IServiceCollection AddHareMessage<
        TMessage,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler
    >(
        this IServiceCollection services
    ) where THandler : class, IMessageHandler<TMessage>
        => AddKeyedHareMessage<TMessage, THandler>(services, key: null);
}