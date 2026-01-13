using System.Diagnostics.CodeAnalysis;
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
internal sealed class JsonEnvelopeSerializer(
    IOptions<HareOptions> options
) : IEnvelopeSerializer
{
    /// <inheritdoc />
    [SuppressMessage(
        "Trimming",
        "IL2026:Members annotated with \'RequiresUnreferencedCodeAttribute\' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "Type information is provided through JsonSerializerOptions"
    )]
    [SuppressMessage(
        "AOT",
        "IL3050:Calling members annotated with \'RequiresDynamicCodeAttribute\' may break functionality when AOT compiling.",
        Justification = "Type information is provided through JsonSerializerOptions"
    )]
    public ValueTask<byte[]> SerializeAsync(Envelope envelope, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(envelope, options.Value.JsonSerializerOptions);
        return ValueTask.FromResult(json);
    }

    /// <inheritdoc />
    [SuppressMessage(
        "Trimming",
        "IL2026:Members annotated with \'RequiresUnreferencedCodeAttribute\' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "Type information is provided through JsonSerializerOptions"
    )]
    [SuppressMessage(
        "AOT",
        "IL3050:Calling members annotated with \'RequiresDynamicCodeAttribute\' may break functionality when AOT compiling.",
        Justification = "Type information is provided through JsonSerializerOptions"
    )]
    public ValueTask<Envelope> DeserializeAsync(ReadOnlySpan<byte> data, CancellationToken cancellationToken)
    {
        var envelope = JsonSerializer.Deserialize<Envelope>(data, options.Value.JsonSerializerOptions);
        if (envelope is null)
            throw new SerializationException("Failed to deserialize envelope from JSON data");

        return ValueTask.FromResult(envelope);
    }
}