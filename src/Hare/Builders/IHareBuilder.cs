using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Hare.Contracts;
using Hare.Contracts.Routing;
using Hare.Routing;

using Microsoft.Extensions.DependencyInjection;

namespace Hare.Builders;

/// <summary>
/// A builder for configuring Hare messaging services.
/// </summary>
public interface IHareBuilder
{
    /// <summary>
    /// Gets the service collection being configured.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Enables conventional routing using the <see cref="DefaultRoutingConvention"/>.
    /// </summary>
    IHareBuilder WithConventionalRouting();

    /// <summary>
    /// Adds <paramref name="context"/> to the chain of type resolvers used by Hare for message serialization.
    /// </summary>
    /// <param name="context">The JSON type information to include.</param>
    /// <remarks>
    /// See <a href="https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation">
    /// Source generation in System.Text.Json</a> for more information.
    /// </remarks>
    IHareBuilder WithJsonSerializerContext(JsonSerializerContext context);

    /// <summary>
    /// Enables automatic provisioning of all messages unless they specify otherwise.
    /// </summary>
    /// <param name="shouldProvision">Whether to enable automatic provisioning.</param>
    IHareBuilder WithAutoProvisioning(bool shouldProvision = true);

    /// <summary>
    /// Enables conventional routing using a custom <see cref="IRoutingConvention"/> implementation.
    /// </summary>
    /// <typeparam name="TConvention">The type of routing convention to use.</typeparam>
    IHareBuilder WithConventionalRouting<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TConvention
    >() where TConvention : class, IRoutingConvention;

    /// <summary>
    /// Adds a message type that Hare can send.
    /// </summary>
    /// <typeparam name="TMessage">The message type to register.</typeparam>
    IHareMessageBuilder<TMessage> AddHareMessage<TMessage>();

    /// <summary>
    /// Adds a message type that Hare can send / receive.
    /// </summary>
    /// <typeparam name="TMessage">The message type to register.</typeparam>
    /// <typeparam name="THandler">The handler type that processes incoming messages.</typeparam>
    IHareMessageBuilder<TMessage> AddHareMessage<
        TMessage,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler
    >() where THandler : class, IMessageHandler<TMessage>;
}