using System.Runtime.Serialization;
using System.Text.Json;

using Hare.Configuration;
using Hare.Contracts.Serialization;

using Microsoft.Extensions.Options;

namespace Hare.Infrastructure.Serialization;

/// <summary>
/// Implements <see cref="IMessageSerializer{TMessage}"/> using System.Text.Json.
/// </summary>
public sealed class JsonMessageSerializer<TMessage>(
    IOptions<MessageOptions<TMessage>> options
) : IMessageSerializer<TMessage>
{
    /// <inheritdoc />
    public ValueTask<byte[]> SerializeAsync(TMessage message, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(message, options.Value.JsonSerializerOptions);
        return ValueTask.FromResult(json);
    }

    /// <inheritdoc />
    public ValueTask<TMessage> DeserializeAsync(ReadOnlySpan<byte> data, CancellationToken cancellationToken)
    {
        var envelope = JsonSerializer.Deserialize<TMessage>(data, options.Value.JsonSerializerOptions);
        if (envelope is null)
            throw new SerializationException($"Failed to deserialize {typeof(TMessage)} from JSON data");

        return ValueTask.FromResult(envelope);
    }
}