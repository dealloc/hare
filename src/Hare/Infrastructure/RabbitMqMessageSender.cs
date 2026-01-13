using System.Diagnostics;

using Hare.Configuration;
using Hare.Contracts;
using Hare.Contracts.Serialization;
using Hare.Models;

using Microsoft.Extensions.Options;

using RabbitMQ.Client;

namespace Hare.Infrastructure;

/// <summary>
/// RabbitMQ implementation of <see cref="IMessageSender{TMessage}" />.
/// </summary>
public sealed class RabbitMqMessageSender<TMessage>(
    IConnection connection,
    IMessageSerializer<TMessage> serializer,
    IOptions<HareOptions> hareOptions,
    IOptions<MessageSendOptions<TMessage>> sendOptions
) : IMessageSender<TMessage>
{
    private static readonly ActivitySource _source = new($"{Constants.ACTIVITY_PREFIX}.RabbitMqMessageSender#{typeof(TMessage).Name}");

    /// <inheritdoc />
    public ValueTask SendAsync(TMessage message, CancellationToken cancellationToken)
        => SendAsync(message, default, cancellationToken);

    /// <inheritdoc />
    public async ValueTask SendAsync(TMessage message, MessageOptions options, CancellationToken cancellationToken)
    {
        using var activity = _source.StartActivity(nameof(SendAsync), ActivityKind.Producer);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        var properties = new BasicProperties
        {
            ContentType = serializer.ContentType,
            CorrelationId = Activity.Current?.Id,
            Expiration = options.Expiration?.Seconds.ToString(),
            MessageId = Guid.NewGuid().ToString(),
            Type = typeof(TMessage).AssemblyQualifiedName,
            AppId = hareOptions.Value.ApplicationName,
            DeliveryMode = options.Persistent ? DeliveryModes.Persistent : DeliveryModes.Transient
        };

        var payload = await serializer.SerializeAsync(message, cancellationToken);
        await channel.BasicPublishAsync(
            exchange: sendOptions.Value.Exchange,
            routingKey: sendOptions.Value.RoutingKey,
            mandatory: sendOptions.Value.Mandatory,
            basicProperties: properties,
            body: payload,
            cancellationToken: cancellationToken
        );
    }
}