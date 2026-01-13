using System.Reflection;
using System.Text.Json;

namespace Hare.Configuration;

/// <summary>
/// Contains configuration options for Hare's message handling.
/// </summary>
public sealed class HareOptions
{
    /// <summary>
    /// The name of the application to attach as metadata to messages.
    /// </summary>
    /// <remarks>
    /// This allows identifying senders in a multiservice system.
    /// </remarks>
    public string ApplicationName { get; set; } = Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty;

    /// <summary>
    /// Used for (de)serialization of message envelopes when using JSON.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new();

    /// <summary>
    /// Whether <b>ALL</b> registered messages should be automatically provisioned.
    /// </summary>
    /// <remarks>
    /// You can override this on a per-message basis by setting <see cref="MessageOptions{TMessage}.AutoProvision"/>.
    /// </remarks>
    public bool AutoProvision { get; set; } = false;
}