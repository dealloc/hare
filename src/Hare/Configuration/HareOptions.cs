using System.Text.Json;

using Hare.Infrastructure.Serialization;

namespace Hare.Configuration;

/// <summary>
/// Contains configuration options for Hare's message handling.
/// </summary>
public sealed class HareOptions
{
    /// <summary>
    /// Used for (de)serialization of message envelopes when using JSON.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new()
    {
        TypeInfoResolverChain = { HareJsonSerializerContext.Default }
    };

    /// <summary>
    /// Whether <b>ALL</b> registered messages should be automatically provisioned.
    /// </summary>
    /// <remarks>
    /// You can override this on a per-message basis by setting <see cref="MessageOptions{TMessage}.AutoProvision"/>.
    /// </remarks>
    public bool AutoProvision { get; set; } = false;
}