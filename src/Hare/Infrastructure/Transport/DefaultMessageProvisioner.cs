using Hare.Configuration;
using Hare.Contracts.Transport;

using Microsoft.Extensions.Options;

using RabbitMQ.Client;

namespace Hare.Infrastructure.Transport;

/// <summary>
/// Default implementation of <see cref="IMessageProvisioner"/> for <typeparamref name="TMessage"/>.
/// </summary>
public sealed class DefaultMessageProvisioner<TMessage>(
    IOptions<MessageOptions<TMessage>> options,
    IOptions<HareOptions> hareOptions,
    IOptions<MessageSendOptions<TMessage>>? sendOptions,
    IOptions<MessageReceiveOptions<TMessage>>? receiveOptions,
    IConnection connection
) : IMessageProvisioner
{
    /// <inheritdoc />
    public async ValueTask ProvisionAsync(CancellationToken cancellationToken = default)
    {
        var shouldProvision = options.Value.AutoProvision ?? hareOptions.Value.AutoProvision;
        if (shouldProvision is false)
            return;

        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        if (sendOptions is not null && string.IsNullOrWhiteSpace(sendOptions.Value.Exchange) is false)
        {
            await channel.ExchangeDeclareAsync(
                exchange: sendOptions.Value.Exchange,
                type: sendOptions.Value.ExchangeType,
                durable: sendOptions.Value.Durable,
                autoDelete: sendOptions.Value.AutoDelete,
                arguments: sendOptions.Value.Arguments,
                passive: sendOptions.Value.Passive,
                noWait: sendOptions.Value.NoWait,
                cancellationToken: cancellationToken
            );
        }

        if (receiveOptions is not null)
        {
            var arguments = receiveOptions.Value.Arguments;
            if (receiveOptions.Value.UseDeadLettering && string.IsNullOrWhiteSpace(receiveOptions.Value.DeadLetterExchangeName) is false)
            {
                arguments["x-dead-letter-exchange"] = receiveOptions.Value.DeadLetterExchangeName;
                arguments["x-dead-letter-routing-key"] = receiveOptions.Value.DeadLetterRoutingKey;

                await channel.ExchangeDeclareAsync(
                    exchange: receiveOptions.Value.DeadLetterExchangeName,
                    type: receiveOptions.Value.DeadLetterExchangeType ?? ExchangeType.Direct,
                    durable: receiveOptions.Value.Durable,
                    autoDelete: receiveOptions.Value.AutoDelete,
                    arguments: new Dictionary<string, object?>(),
                    passive: receiveOptions.Value.Passive,
                    noWait: receiveOptions.Value.NoWait,
                    cancellationToken: cancellationToken
                );

                var dlq = await channel.QueueDeclareAsync(
                    queue: receiveOptions.Value.DeadLetterRoutingKey ??
                           throw new ArgumentNullException(nameof(receiveOptions.Value.DeadLetterRoutingKey)),
                    durable: receiveOptions.Value.Durable,
                    exclusive: receiveOptions.Value.Exclusive,
                    autoDelete: receiveOptions.Value.AutoDelete,
                    arguments: new Dictionary<string, object?>(),
                    passive: receiveOptions.Value.Passive,
                    noWait: receiveOptions.Value.NoWait,
                    cancellationToken: cancellationToken
                );

                if (string.IsNullOrWhiteSpace(receiveOptions.Value.DeadLetterExchangeName) is false)
                {
                    await channel.QueueBindAsync(
                        queue: dlq.QueueName,
                        exchange: receiveOptions.Value.DeadLetterExchangeName,
                        routingKey: receiveOptions.Value.DeadLetterRoutingKey ?? dlq.QueueName,
                        noWait: receiveOptions.Value.NoWait,
                        cancellationToken: cancellationToken
                    );
                }
            }

            var queue = await channel.QueueDeclareAsync(
                queue: receiveOptions.Value.QueueName ??
                       throw new ArgumentNullException(nameof(receiveOptions.Value.QueueName)),
                durable: receiveOptions.Value.Durable,
                exclusive: receiveOptions.Value.Exclusive,
                autoDelete: receiveOptions.Value.AutoDelete,
                arguments: arguments,
                passive: receiveOptions.Value.Passive,
                noWait: receiveOptions.Value.NoWait,
                cancellationToken: cancellationToken
            );

            if (string.IsNullOrWhiteSpace(receiveOptions.Value.Exchange) is false)
            {
                await channel.QueueBindAsync(
                    queue: queue.QueueName,
                    exchange: receiveOptions.Value.Exchange,
                    routingKey: receiveOptions.Value.RoutingKey ?? queue.QueueName,
                    noWait: receiveOptions.Value.NoWait,
                    cancellationToken: cancellationToken
                );
            }
        }
    }
}