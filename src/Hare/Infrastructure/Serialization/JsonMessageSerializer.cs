using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json;

using Hare.Configuration;
using Hare.Contracts.Serialization;

using Microsoft.Extensions.Options;

namespace Hare.Infrastructure.Serialization;

/// <summary>
/// Implements <see cref="IMessageSerializer{TMessage}"/> using System.Text.Json.
/// </summary>
internal sealed class JsonMessageSerializer<TMessage>(
    IOptions<MessageOptions<TMessage>> options,
    IOptions<HareOptions> hareOptions
) : IMessageSerializer<TMessage>
{
    /// <inheritdoc />
    public string? ContentType => "application/json";

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
    public ValueTask<byte[]> SerializeAsync(TMessage message, CancellationToken cancellationToken)
    {
        var serializerOptions = options.Value.JsonSerializerOptions ?? hareOptions.Value.JsonSerializerOptions;
        var json = JsonSerializer.SerializeToUtf8Bytes(message, serializerOptions);
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
    public ValueTask<TMessage> DeserializeAsync(ReadOnlySpan<byte> data, CancellationToken cancellationToken)
    {
        var serializerOptions = options.Value.JsonSerializerOptions ?? hareOptions.Value.JsonSerializerOptions;
        var envelope = JsonSerializer.Deserialize<TMessage>(data, serializerOptions);
        if (envelope is null)
            throw new SerializationException($"Failed to deserialize {typeof(TMessage)} from JSON data");

        return ValueTask.FromResult(envelope);
    }
}