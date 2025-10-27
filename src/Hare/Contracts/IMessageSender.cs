namespace Hare.Contracts;

/// <summary>
/// Used to dispatch messages to RabbitMQ.
/// </summary>
/// <remarks>For performance reasons, re-use the same scoped instance of the sender to prevent channel churn.</remarks>
public interface IMessageSender<in TMessage> where TMessage : class
{
    /// <summary>
    /// Send the <typeparamref name="TMessage" /> to RabbitMQ.
    /// </summary>
    public ValueTask SendMessageAsync(TMessage message, CancellationToken cancellationToken);
}