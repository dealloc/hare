using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Hare.Configuration;
using Hare.Contracts;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Hare.Builders;

/// <summary>
/// Default implementation of <see cref="IHareMessageBuilder{TMessage}"/>.
/// </summary>
internal sealed class HareMessageBuilder<TMessage>(HareBuilder parent) : IHareMessageBuilder<TMessage>
{
    /// <inheritdoc />
    public IServiceCollection Services => parent.Services;

    /// <inheritdoc />
    public IHareMessageBuilder<TMessage> WithQueue(string queueName)
    {
        // PostConfigure runs after IConfigureOptions, allowing overrides of convention-based values
        Services.PostConfigure<MessageReceiveOptions<TMessage>>(options =>
        {
            options.QueueName = queueName;
        });

        Services.PostConfigure<MessageSendOptions<TMessage>>(options =>
        {
            // If routing key wasn't explicitly set, default to queue name for direct routing
            if (string.IsNullOrEmpty(options.Exchange))
                options.RoutingKey = queueName;
        });

        return this;
    }

    /// <inheritdoc />
    public IHareMessageBuilder<TMessage> WithDeadLetter(bool useDeadLettering = true)
    {
        // PostConfigure runs after IConfigureOptions, allowing overrides of convention-based values
        Services.PostConfigure<MessageReceiveOptions<TMessage>>(options =>
        {
            options.UseDeadLettering = useDeadLettering;
        });

        return this;
    }

    /// <inheritdoc />
    public IHareMessageBuilder<TMessage> WithExchange(string exchange, string exchangeType = "direct")
    {
        Services.PostConfigure<MessageSendOptions<TMessage>>(options =>
        {
            options.Exchange = exchange;
            options.ExchangeType = exchangeType;
        });

        Services.PostConfigure<MessageReceiveOptions<TMessage>>(options =>
        {
            options.Exchange = exchange;
        });

        return this;
    }

    /// <inheritdoc />
    public IHareMessageBuilder<TMessage> WithDeadLetterExchange(string exchange, string exchangeType = "direct")
    {
        Services.PostConfigure<MessageReceiveOptions<TMessage>>(options =>
        {
            options.UseDeadLettering = true;
            options.DeadLetterExchangeName = exchange;
            options.DeadLetterExchangeType = exchangeType;
        });

        return this;
    }

    /// <inheritdoc />
    public IHareMessageBuilder<TMessage> WithRoutingKey(string routingKey)
    {
        Services.PostConfigure<MessageSendOptions<TMessage>>(options =>
        {
            options.RoutingKey = routingKey;
        });

        Services.PostConfigure<MessageReceiveOptions<TMessage>>(options =>
        {
            options.RoutingKey = routingKey;
        });

        return this;
    }

    /// <inheritdoc />
    public IHareMessageBuilder<TMessage> WithDeadLetterRoutingKey(string routingKey)
    {
        Services.PostConfigure<MessageReceiveOptions<TMessage>>(options =>
        {
            options.UseDeadLettering = true;
            options.DeadLetterRoutingKey = routingKey;
        });

        return this;
    }

    /// <inheritdoc />
    public IHareMessageBuilder<TMessage> WithConcurrency(ulong concurrency)
    {
        Services.PostConfigure<MessageOptions<TMessage>>(options =>
        {
            options.ConcurrentListeners = concurrency;
        });

        return this;
    }

    /// <inheritdoc />
    public IHareMessageBuilder<TMessage> WithJsonSerializerContext(JsonSerializerContext context)
    {
        Services.Configure<MessageOptions<TMessage>>(options =>
        {
            options.JsonSerializerOptions ??= new();
            options.JsonSerializerOptions.TypeInfoResolverChain.Add(context);
        });

        return this;
    }

    /// <inheritdoc />
    public IHareMessageBuilder<TMessage> WithAutoProvisioning(bool? shouldProvision)
    {
        Services.Configure<MessageOptions<TMessage>>(options => options.AutoProvision = shouldProvision);

        return this;
    }

    /// <inheritdoc />
    public IHareMessageBuilder<TOtherMessage> AddHareMessage<TOtherMessage>()
        => parent.AddHareMessage<TOtherMessage>();

    /// <inheritdoc />
    public IHareMessageBuilder<TOtherMessage> AddHareMessage<
        TOtherMessage,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler
    >() where THandler : class, IMessageHandler<TOtherMessage>
        => parent.AddHareMessage<TOtherMessage, THandler>();
}