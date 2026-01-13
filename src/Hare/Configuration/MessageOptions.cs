using System.Text.Json;

namespace Hare.Configuration;

/// <summary>
/// Contains configuration options for handling messages of <typeparamref name="TMessage"/>.
/// </summary>
public sealed class MessageOptions<TMessage>
{
    /// <summary>
    /// Used for (de)serialization of <typeparamref name="TMessage"/> messages when using JSON.
    /// If set to <c>null</c> the <see cref="HareOptions.JsonSerializerOptions"/> will be used instead.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// The number of concurrent listeners that should be started for this message type.
    /// </summary>
    public ulong ConcurrentListeners { get; set; } = 1;

    /// <summary>
    /// Whether the destination of this message should be automatically provisioned.
    /// </summary>
    /// <remarks>
    /// If you set this to <c>null</c> (which is the default), the <see cref="HareOptions.AutoProvision"/> setting will be used instead.
    /// </remarks>
    public bool? AutoProvision { get; set; } = null;
}