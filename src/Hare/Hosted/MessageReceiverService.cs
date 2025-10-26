using System.Diagnostics;
using System.Text.Json;

using Hare.Configuration;
using Hare.Contracts;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Hare.Hosted;

/// <summary>
/// Handles receiving messages from the queue and dispatching them to the handlers.
/// </summary>
public sealed partial class MessageReceiverService<TMessage>(
    ILogger<MessageReceiverService<TMessage>> logger,
    IServiceScopeFactory scopeFactory,
    IOptions<HareMessageConfiguration<TMessage>> messageConfiguration
) : BackgroundService where TMessage : class
{
    private CancellationToken? _cancellationToken;
    private static readonly ActivitySource Source = new(nameof(MessageReceiverService<TMessage>));

    [LoggerMessage(LogLevel.Error, Message = "An error occured processing a {MessageType} message")]
    private static partial void LogMessageHandlerError(ILogger logger, Exception exception, Type messageType);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        await using var scope = scopeFactory.CreateAsyncScope();
        var connection = scope.ServiceProvider.GetRequiredService<IConnection>();

        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(
            queue: messageConfiguration.Value.QueueName,
            durable: messageConfiguration.Value.Durable,
            exclusive: messageConfiguration.Value.Exclusive,
            autoDelete: messageConfiguration.Value.AutoDelete,
            arguments: messageConfiguration.Value.Arguments,
            cancellationToken: cancellationToken
        );

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += ConsumerOnReceivedAsync;

        await channel.BasicConsumeAsync(
            messageConfiguration.Value.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken
        );
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private async Task ConsumerOnReceivedAsync(object sender, BasicDeliverEventArgs @event)
    {
        using var activity = Source.StartActivity(typeof(TMessage).FullName ?? typeof(TMessage).Name);

        // Link the handler to the sender
        if (@event.BasicProperties.CorrelationId is not null)
            activity?.SetParentId(@event.BasicProperties.CorrelationId);

        var channel = (sender as AsyncEventingBasicConsumer)!.Channel;
        await using var scope = scopeFactory.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetService<IMessageHandler<TMessage>>();

        try
        {
            if (handler is null)
                throw new InvalidOperationException(
                    $"No handler found for message type {typeof(TMessage).FullName ?? typeof(TMessage).Name}");

            using var memory = new MemoryStream(@event.Body.ToArray());
            var message = await JsonSerializer.DeserializeAsync(
                memory,
                messageConfiguration.Value.JsonTypeInfo
            );

            if (message is null)
                throw new InvalidOperationException("Message could not be deserialized");

            await handler.HandleAsync(message!, _cancellationToken.GetValueOrDefault());
            await channel.BasicAckAsync(
                @event.DeliveryTag,
                multiple: false,
                cancellationToken: _cancellationToken.GetValueOrDefault()
            );
        }
        catch (Exception exception)
        {
            activity?.AddException(exception);
            LogMessageHandlerError(logger, exception, typeof(TMessage));
        }
    }
}