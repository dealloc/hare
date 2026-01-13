using Hare.Models;

namespace Hare.Contracts.Serialization;

/// <summary>
/// Handles serialization and deserialization of envelopes before handing them over to the transport layer.
/// </summary>
public interface IEnvelopeSerializer
{
    /// <summary>
    /// Serializes the envelope to a byte array.
    /// </summary>
    /// <param name="envelope">The envelope to serialize.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    ValueTask<byte[]> SerializeAsync(Envelope envelope, CancellationToken cancellationToken);

    /// <summary>
    /// Deserializes the envelope from a byte array.
    /// </summary>
    /// <param name="data">The byte array to deserialize the envelope from.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    ValueTask<Envelope> DeserializeAsync(ReadOnlySpan<byte> data, CancellationToken cancellationToken);
}