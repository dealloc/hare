using Hare.Configuration;
using Hare.Contracts;
using Hare.Contracts.Serialization;

using Microsoft.Extensions.Options;

using RabbitMQ.Client;

namespace Hare.Infrastructure.Transport;

/// <summary>
/// RabbitMQ implementation of <see cref="IMessageSender{TMessage}" />.
/// </summary>
public sealed class RabbitMqMessageSender<TMessage>(
    IConnection connection,
    IOptions<MessageSendOptions<TMessage>> options
) : IMessageSender<TMessage>
{
    /// <inheritdoc />
    public async ValueTask SendAsync(TMessage message, CancellationToken cancellationToken)
    {
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.BasicPublishAsync(
            exchange: options.Value.Exchange,
            routingKey: options.Value.RoutingKey,
            mandatory: options.Value.Mandatory,
            basicProperties: new BasicProperties(),
            body: Memory<byte>.Empty,
            cancellationToken: cancellationToken
        );
    }
}