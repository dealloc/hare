using System.Text.Json;

namespace Hare.Configuration;

/// <summary>
/// Contains configuration options for Hare's message handling.
/// </summary>
public sealed class HareOptions
{
    /// <summary>
    /// Used for (de)serialization of message envelopes when using JSON.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new();
}