using System.Diagnostics;
using System.Text.Json;

using Hare.Configuration;
using Hare.Contracts;

using Microsoft.Extensions.Options;

using RabbitMQ.Client;

namespace Hare.Services;

public class DefaultMessageSender<TMessage>(
    IConnection connection,
    IOptions<HareMessageConfiguration<TMessage>> messageConfiguration
) : IMessageSender<TMessage>, IAsyncDisposable where TMessage : class
{
    private readonly Task<IChannel> _channel = InitializeAsync(connection, messageConfiguration);

    private static async Task<IChannel> InitializeAsync(IConnection connection, IOptions<HareMessageConfiguration<TMessage>> messageConfiguration)
    {
        var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync(
            queue: messageConfiguration.Value.QueueName,
            durable: messageConfiguration.Value.Durable,
            exclusive: messageConfiguration.Value.Exclusive,
            autoDelete: messageConfiguration.Value.AutoDelete,
            arguments: messageConfiguration.Value.Arguments
        );

        return channel;
    }

    /// <inheritdoc />
    public async ValueTask SendMessageAsync(TMessage message, CancellationToken cancellationToken)
    {
        var channel = await _channel;

        using var memory = new MemoryStream();
        await JsonSerializer.SerializeAsync(
            memory,
            message,
            messageConfiguration.Value.JsonTypeInfo,
            cancellationToken
        );

        await channel.BasicPublishAsync(
            exchange: messageConfiguration.Value.Exchange,
            routingKey: messageConfiguration.Value.QueueName,
            mandatory: false,
            basicProperties: new BasicProperties
            {
                CorrelationId = Activity.Current?.Id
            },
            body: memory.ToArray(),
            cancellationToken: cancellationToken
        );
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        var channel = await _channel;

        await channel.DisposeAsync();;
    }
}