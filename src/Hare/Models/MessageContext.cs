using RabbitMQ.Client;

namespace Hare.Models;

/// <summary>
/// Contains additional contextual information for a message.
/// </summary>
public readonly struct MessageContext
{
    /// <summary>
    /// Whether this message was redelivered.
    /// </summary>
    public required bool Redelivered { get; init; }

    /// <summary>
    /// The name of the exchange to which the message was sent.
    /// </summary>
    public required string Exchange { get; init; }

    /// <summary>
    /// The routing key used when sending the message.
    /// </summary>
    public required string RoutingKey { get; init; }

    /// <summary>
    /// The message properties.
    /// </summary>
    public required IReadOnlyBasicProperties Properties { get; init; }

    /// <summary>
    /// The raw message payload as it was received.
    /// </summary>
    public required ReadOnlyMemory<byte> Payload { get; init; }
};