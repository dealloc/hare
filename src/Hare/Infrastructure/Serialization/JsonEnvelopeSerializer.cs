using System.Runtime.Serialization;
using System.Text.Json;

using Hare.Configuration;
using Hare.Contracts.Serialization;
using Hare.Models;

using Microsoft.Extensions.Options;

namespace Hare.Infrastructure.Serialization;

/// <summary>
/// Implements <see cref="IEnvelopeSerializer"/> using System.Text.Json.
/// </summary>
public sealed class JsonEnvelopeSerializer(
    IOptions<HareOptions> options
) : IEnvelopeSerializer
{
    /// <inheritdoc />
    public ValueTask<byte[]> Serialize(Envelope envelope)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(envelope, options.Value.JsonSerializerOptions);
        return ValueTask.FromResult(json);
    }

    /// <inheritdoc />
    public ValueTask<Envelope> Deserialize(ReadOnlySpan<byte> data)
    {
        var envelope = JsonSerializer.Deserialize<Envelope>(data, options.Value.JsonSerializerOptions);
        if (envelope is null)
            throw new SerializationException("Failed to deserialize envelope from JSON data");

        return ValueTask.FromResult(envelope);
    }
}