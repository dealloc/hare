using Hare.Configuration;
using Hare.Contracts.Transport;

using Microsoft.Extensions.Options;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Hare.Infrastructure.Transport;

/// <summary>
/// A simple <see cref="IListener{TMessage}" /> that uses RabbitMQ as the transport layer.
/// </summary>
public sealed class RabbitMqListener<TMessage>(
    IConnection connection,
    IOptions<MessageReceiveOptions<TMessage>> receiveOptions
) : IListener<TMessage>, IAsyncDisposable
{
    private IChannel? _channel;
    private AsyncEventingBasicConsumer? _consumer;

    /// <inheritdoc />
    public async Task ListenForIncomingMessagesAsync(CancellationToken cancellationToken)
    {
        _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await _channel.QueueDeclarePassiveAsync(
            receiveOptions.Value.QueueName
            ?? throw new ArgumentNullException(nameof(receiveOptions.Value.QueueName),
                $"Queue name of {typeof(TMessage).FullName} should not be null."),
            cancellationToken: cancellationToken
        );

        _consumer = new AsyncEventingBasicConsumer(_channel);
        _consumer.ReceivedAsync += onMessageReceivedAsync;

        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }

    /// <summary>
    /// Called when RabbitMQ receives a message.
    /// </summary>
    private Task onMessageReceivedAsync(object sender, BasicDeliverEventArgs @event)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();
    }
}