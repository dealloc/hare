using System.Text.Json;

using Hare.Configuration;
using Hare.Contracts;

using Microsoft.Extensions.Options;

using RabbitMQ.Client;

namespace Hare.Services;

public class DefaultMessageSender<TMessage>(
    IConnection connection,
    IOptions<HareMessageConfiguration<TMessage>> messageConfiguration
) : IMessageSender<TMessage> where TMessage : class
{
    /// <inheritdoc />
    public async ValueTask SendMessageAsync(TMessage message, CancellationToken cancellationToken)
    {
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(
            queue: messageConfiguration.Value.QueueName,
            durable: messageConfiguration.Value.Durable,
            exclusive: messageConfiguration.Value.Exclusive,
            autoDelete: messageConfiguration.Value.AutoDelete,
            arguments: messageConfiguration.Value.Arguments,
            cancellationToken: cancellationToken
        );

        using var memory = new MemoryStream();
        await JsonSerializer.SerializeAsync(
            memory,
            message,
            messageConfiguration.Value.JsonTypeInfo,
            cancellationToken
        );

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: string.Empty,
            mandatory: false,
            basicProperties: new BasicProperties(),
            body: memory.ToArray(),
            cancellationToken: cancellationToken
        );
    }
}