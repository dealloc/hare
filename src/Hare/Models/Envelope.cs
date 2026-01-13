namespace Hare.Models;

/// <summary>
/// A wrapper type around a message payload, allows passing metadata alongside the payload.
/// </summary>
/// <param name="MessageId">A globally unique identifier for this message.</param>
/// <param name="CorrelationId">The correlation ID for correlating the handler with the activity that sent it.</param>
/// <param name="MessageType">The type of the message payload should resolve to a valid type or set to <c>null</c>.</param>
/// <param name="Timestamp">The timestamp when this message was created, relative to UTC.</param>
/// <param name="Payload">The message payload as a byte array.</param>
public record Envelope(
    string MessageId,
    string? CorrelationId,
    string? MessageType,
    DateTimeOffset Timestamp,
    byte[] Payload
);
