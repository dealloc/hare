namespace Hare.Models;

/// <summary>
/// Allows passing additional options to the transport layer sending the message.
/// </summary>
public struct MessageOptions
{
    /// <summary>
    /// Allows passing in a TTL for the message.
    /// Messages past their TTL will be discarded.
    /// </summary>
    public TimeSpan? Expiration { get; set; }

    /// <summary>
    /// Whether the message should be sent as persistent.
    /// </summary>
    public bool Persistent { get; set; }
}