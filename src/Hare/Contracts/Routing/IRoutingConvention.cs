namespace Hare.Contracts.Routing;

/// <summary>
/// Defines conventions for automatically deriving routing configuration from message types.
/// </summary>
public interface IRoutingConvention
{
    /// <summary>
    /// Gets the queue name for a message type.
    /// </summary>
    /// <param name="messageType">The type of message.</param>
    string GetQueueName(Type messageType);

    /// <summary>
    /// Gets the routing key for a message type.
    /// </summary>
    /// <param name="messageType">The type of message.</param>
    string GetRoutingKey(Type messageType);

    /// <summary>
    /// Gets the routing key for a message type's dead-letter.
    /// </summary>
    /// <param name="messageType">The type of message.</param>
    string GetDeadLetterRoutingKey(Type messageType);

    /// <summary>
    /// Gets the exchange name for a message type.
    /// </summary>
    /// <param name="messageType">The type of message.</param>
    string GetExchange(Type messageType);

    /// <summary>
    /// Gets the exchange name for a message type's dead-letter.
    /// </summary>
    /// <param name="messageType">The type of message.</param>
    string GetDeadLetterExchange(Type messageType);

    /// <summary>
    /// Gets the exchange type for a message type.
    /// </summary>
    /// <param name="messageType">The type of message.</param>
    string GetExchangeType(Type messageType);

    /// <summary>
    /// Gets the exchange type for a message type's dead-letter.
    /// </summary>
    /// <param name="messageType">The type of message.</param>
    string GetDeadLetterExchangeType(Type messageType);
}