using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Hare.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace Hare.Builders;

/// <summary>
/// A builder for configuring a specific message type in Hare.
/// </summary>
/// <typeparam name="TMessage">The message type being configured.</typeparam>
public interface IHareMessageBuilder<TMessage>
{
    /// <summary>
    /// Gets the service collection being configured.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Sets the queue name for this message, overriding any convention.
    /// </summary>
    /// <param name="queueName">The queue name to use.</param>
    IHareMessageBuilder<TMessage> WithQueue(string queueName);

    /// <summary>
    /// Sets the exchange for this message, overriding any convention.
    /// </summary>
    /// <param name="exchange">The exchange name to use.</param>
    /// <param name="exchangeType">The exchange type. Defaults to <c>"direct"</c>.</param>
    IHareMessageBuilder<TMessage> WithExchange(string exchange, string exchangeType = "direct");

    /// <summary>
    /// Sets the routing key for this message, overriding any convention.
    /// </summary>
    /// <param name="routingKey">The routing key to use.</param>
    IHareMessageBuilder<TMessage> WithRoutingKey(string routingKey);

    /// <summary>
    /// Sets the number of concurrent listeners for this message type.
    /// </summary>
    /// <param name="concurrency">The number of concurrent listeners.</param>
    IHareMessageBuilder<TMessage> WithConcurrency(ulong concurrency);

    /// <summary>
    /// Adds <paramref name="context"/> to the chain of type resolvers used by Hare for message serialization.
    /// </summary>
    /// <param name="context">The JSON type information to include.</param>
    /// <remarks>
    /// See <a href="https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation">
    /// Source generation in System.Text.Json</a> for more information.
    /// </remarks>
    IHareMessageBuilder<TMessage> WithJsonSerializerContext(JsonSerializerContext context);

    /// <summary>
    /// Enables automatic provisioning of all messages unless they specify otherwise.
    /// </summary>
    /// <param name="shouldProvision">Whether to enable automatic provisioning, set to <c>null</c> to inherit.</param>
    IHareMessageBuilder<TMessage> WithAutoProvisioning(bool? shouldProvision = true);

    /// <summary>
    /// Adds another message type that Hare can send.
    /// </summary>
    /// <typeparam name="TOtherMessage">The message type to register.</typeparam>
    IHareMessageBuilder<TOtherMessage> AddHareMessage<TOtherMessage>();

    /// <summary>
    /// Adds another message type that Hare can send and receive.
    /// </summary>
    /// <typeparam name="TOtherMessage">The message type to register.</typeparam>
    /// <typeparam name="THandler">The handler type that processes incoming messages.</typeparam>
    IHareMessageBuilder<TOtherMessage> AddHareMessage<
        TOtherMessage,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler
    >() where THandler : class, IMessageHandler<TOtherMessage>;
}