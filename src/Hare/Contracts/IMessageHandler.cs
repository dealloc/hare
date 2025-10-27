namespace Hare.Contracts;

/// <summary>
/// Handles incoming messages from RabbitMQ.
/// </summary>
/// <typeparam name="TMessage">The type of message handled.</typeparam>
public interface IMessageHandler<in TMessage>
{
    /// <summary>
    /// Called when a new message arrives from RabbitMQ.
    /// </summary>
    /// <param name="message">The received message.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> used by the runtime to cancel the handler.</param>
    public ValueTask HandleAsync(TMessage message, CancellationToken cancellationToken);
}