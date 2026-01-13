namespace Hare.Models;


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public record Envelope(
    string MessageId,
    string? CorrelationId,
    string? MessageType,
    DateTimeOffset Timestamp,
    byte[] Payload
);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member