using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Hare.Configuration;
using Hare.Contracts;
using Hare.Contracts.Routing;
using Hare.Extensions;
using Hare.Routing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Hare.Builders;

/// <summary>
/// Default implementation of <see cref="IHareBuilder"/>.
/// </summary>
internal sealed class HareBuilder(IServiceCollection services) : IHareBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; } = services;

    /// <inheritdoc />
    public IHareBuilder WithConventionalRouting()
        => WithConventionalRouting<DefaultRoutingConvention>();

    /// <inheritdoc />
    public IHareBuilder WithJsonSerializerContext(JsonSerializerContext context)
    {
        Services.Configure<HareOptions>(options => options.JsonSerializerOptions.TypeInfoResolverChain.Add(context));

        return this;
    }

    /// <inheritdoc />
    public IHareBuilder WithAutoProvisioning(bool shouldProvision)
    {
        Services.Configure<HareOptions>(options => options.AutoProvision = shouldProvision);

        return this;
    }

    /// <inheritdoc />
    public IHareBuilder WithConventionalRouting<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TConvention
    >() where TConvention : class, IRoutingConvention
    {
        Services.AddSingleton<IRoutingConvention, TConvention>();
        return this;
    }

    /// <inheritdoc />
    public IHareMessageBuilder<TMessage> AddHareMessage<TMessage>()
    {
        RegisterConventionBasedOptions<TMessage>();
        Services.AddHareMessage<TMessage>();

        return new HareMessageBuilder<TMessage>(this);
    }

    /// <inheritdoc />
    public IHareMessageBuilder<TMessage> AddHareMessage<
        TMessage,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    THandler
    >() where THandler : class, IMessageHandler<TMessage>
    {
        Services.AddHareMessage<TMessage, THandler>();

        return new HareMessageBuilder<TMessage>(this);
    }

    private void RegisterConventionBasedOptions<TMessage>()
    {
        // Register IConfigureOptions that applies conventions if IRoutingConvention is available
        Services.AddSingleton<IConfigureOptions<MessageSendOptions<TMessage>>>(provider =>
        {
            var convention = provider.GetService<IRoutingConvention>();
            return new ConfigureOptions<MessageSendOptions<TMessage>>(options =>
            {
                if (convention is null)
                    return;

                var messageType = typeof(TMessage);
                options.RoutingKey = convention.GetRoutingKey(messageType);
                options.Exchange = convention.GetExchange(messageType);
                options.ExchangeType = convention.GetExchangeType(messageType);
            });
        });

        Services.AddSingleton<IConfigureOptions<MessageReceiveOptions<TMessage>>>(provider =>
        {
            var convention = provider.GetService<IRoutingConvention>();
            return new ConfigureOptions<MessageReceiveOptions<TMessage>>(options =>
            {
                if (convention is null)
                    return;

                var messageType = typeof(TMessage);
                options.QueueName = convention.GetQueueName(messageType);
                options.RoutingKey = convention.GetRoutingKey(messageType);
                options.Exchange = convention.GetExchange(messageType);
            });
        });
    }
}