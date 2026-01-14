using System.Diagnostics;

using Hare.Configuration;
using Hare.Contracts;
using Hare.Contracts.Serialization;
using Hare.Contracts.Transport;
using Hare.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Hare.Infrastructure.Transport;

/// <summary>
/// A simple <see cref="IListener{TMessage}" /> that uses RabbitMQ as the transport layer.
/// </summary>
public sealed class RabbitMqListener<TMessage>(
    IConnection connection,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqListener<TMessage>> logger,
    IOptions<MessageReceiveOptions<TMessage>> receiveOptions,
    IMessageSerializer<TMessage> serializer
) : IListener<TMessage>, IAsyncDisposable
{
    private IChannel? _channel;
    private AsyncEventingBasicConsumer? _consumer;
    private static readonly ActivitySource _source = new($"{Constants.ACTIVITY_PREFIX}.RabbitMqListener#{typeof(TMessage).Name}");

    /// <inheritdoc />
    public async Task ListenForIncomingMessagesAsync(CancellationToken cancellationToken)
    {
        _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        try
        {
            await _channel.QueueDeclarePassiveAsync(
                receiveOptions.Value.QueueName
                ?? throw new ArgumentNullException(nameof(receiveOptions.Value.QueueName),
                    $"Queue name of {typeof(TMessage).FullName} should not be null."),
                cancellationToken: cancellationToken
            );
        }
        catch (OperationInterruptedException exception) when (exception.ShutdownReason?.ReplyCode is 404)
        {
            throw new InvalidOperationException($"Queue {receiveOptions.Value.QueueName} does not exist.", exception);
        }

        _consumer = new AsyncEventingBasicConsumer(_channel);
        _consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: receiveOptions.Value.QueueName,
            autoAck: false,
            consumer: _consumer,
            cancellationToken: cancellationToken
        );

        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }

    /// <summary>
    /// Called when RabbitMQ receives a message.
    /// </summary>
    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs @event)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var handlers = scope.ServiceProvider.GetServices<IMessageHandler<TMessage>>();
            var context = new MessageContext
            {
                Redelivered = @event.Redelivered,
                Exchange = @event.Exchange,
                RoutingKey = @event.RoutingKey,
                Properties = @event.BasicProperties,
                Payload = @event.Body
            };
            var message = await serializer.DeserializeAsync(@event.Body, @event.CancellationToken);

            foreach (var handler in handlers)
            {
                using var activity = _source.StartActivity($"{handler.GetType().Name}", ActivityKind.Consumer);
                activity?.AddTag("message.type", typeof(TMessage).AssemblyQualifiedName);
                if (activity is not null && string.IsNullOrWhiteSpace(@event.BasicProperties.CorrelationId) is false)
                    activity.SetParentId(@event.BasicProperties.CorrelationId);

                try
                {
                    logger.LogTrace("Handling message of type {MessageType}", typeof(TMessage).Name);
                    await handler.HandleAsync(message, context, @event.CancellationToken);
                }
                catch (Exception exception)
                {
                    activity?.AddException(exception);
                    activity?.SetStatus(ActivityStatusCode.Error);
                    logger.LogError(exception, "Failed to handle message of type {MessageType}", typeof(TMessage).Name);

                    throw;
                }
            }

            await _channel!.BasicAckAsync(
                deliveryTag: @event.DeliveryTag,
                multiple: false,
                cancellationToken: @event.CancellationToken
            );
        }
        catch (Exception)
        {
            var requeue = @event.Redelivered is false;
            if (requeue is false && receiveOptions.Value.UseDeadLettering)
                logger.LogWarning("Moving message {DeliveryTag} to DLX", @event.DeliveryTag);

            await _channel!.BasicNackAsync(
                deliveryTag: @event.DeliveryTag,
                multiple: false,
                requeue: requeue,
                cancellationToken: @event.CancellationToken
            );
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();
    }
}