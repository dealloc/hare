using System.Diagnostics;
using System.Net.Mime;
using System.Text.Json;

using Hare.Configuration;
using Hare.Contracts;

using Microsoft.Extensions.Options;

using RabbitMQ.Client;

namespace Hare.Services;

/// <summary>
/// Default implementation of the Hare <see cref="IMessageSender{TMessage}" />.
///
/// This implementation also performs optimization by keeping a <see cref="IChannel" /> alive during the scoped lifetime.
/// </summary>
public class DefaultMessageSender<TMessage>(
    IConnection connection,
    IOptions<HareMessageConfiguration<TMessage>> messageConfiguration
) : IMessageSender<TMessage>, IAsyncDisposable where TMessage : class
{
    private readonly Task<IChannel> _channel = InitializeAsync(connection, messageConfiguration);

    private static async Task<IChannel> InitializeAsync(IConnection connection, IOptions<HareMessageConfiguration<TMessage>> messageConfiguration)
    {
        var channel = await connection.CreateChannelAsync();

        var arguments = messageConfiguration.Value.Arguments ?? new Dictionary<string, object?>();
        arguments.TryAdd("x-dead-letter-exchange", messageConfiguration.Value.DeadLetterExchange);
        await channel.QueueDeclareAsync(
            queue: messageConfiguration.Value.QueueName,
            durable: messageConfiguration.Value.Durable,
            exclusive: messageConfiguration.Value.Exclusive,
            autoDelete: messageConfiguration.Value.AutoDelete,
            arguments
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
                CorrelationId = Activity.Current?.Id,
                ContentType = "application/json",
                ContentEncoding = "UTF-8"
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