namespace Hare.Contracts.Serialization;

/// <summary>
/// Handles serialization and deserialization of messages before handing them over to the transport layer.
/// </summary>
/// <typeparam name="TMessage">The type of message to serialize and deserialize.</typeparam>
public interface IMessageSerializer<TMessage>
{
    /// <summary>
    /// Returns the MIME type of the format in which the message is serialized.
    /// </summary>
    string? ContentType { get; }

    /// <summary>
    /// Serializes the message to a byte array.
    /// </summary>
    /// <param name="message">The message to serialize.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    ValueTask<byte[]> SerializeAsync(TMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// Deserializes the message from a byte array.
    /// </summary>
    /// <param name="data">The byte array to deserialize the message from.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    ValueTask<TMessage> DeserializeAsync(ReadOnlySpan<byte> data, CancellationToken cancellationToken);
}