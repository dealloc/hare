namespace Hare.Configuration;

/// <summary>
/// Describes how messages should be received.
/// </summary>
public sealed class MessageReceiveOptions<TMessage>
{
    /// <summary>
    /// The name of the exchange to send the message to.
    /// If set to <c>""</c> will use the default exchange.
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// The name of the queue to receive messages from.
    /// </summary>
    public string? QueueName { get; set; }

    /// <summary>
    /// The routing key to use when binding to the <see cref="Exchange" />.
    /// If set to <c>null</c> the queue name will be used instead.
    /// </summary>
    public string? RoutingKey { get; set; }

    /// <summary>
    /// Should the queue survive a broker restart?
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Should the queue use be limited to its declaring connection?
    /// This will cause the queue to be deleted when the declaring connection closes.
    /// </summary>
    public bool Exclusive { get; set; }

    /// <summary>
    /// Should this queue be auto-deleted when its last consumer unsubscribes.
    /// </summary>
    public bool AutoDelete { get; set; }

    /// <summary>
    /// Optional; additional arguments for the queue declaration.
    /// </summary>
    public Dictionary<string, object?> Arguments { get; set; } = [];

    /// <summary>
    /// Whether the queue should be declared passive.
    /// </summary>
    public bool Passive { get; set; }

    /// <summary>
    /// Set to true to not require a response from the server.
    /// </summary>
    public bool NoWait { get; set; }
}