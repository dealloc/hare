namespace Hare.Configuration;

/// <summary>
/// Describes how messages should be sent.
/// </summary>
public sealed class MessageSendOptions<TMessage>
{
    /// <summary>
    /// The name of the exchange to send the message to.
    /// If set to <c>""</c> will use the default exchange and ignore all other settings.
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// The type of exchange, see <a href="https://www.rabbitmq.com/docs/exchanges#types">RabbitMQ documentation</a>.
    /// </summary>
    public string ExchangeType { get; set; } = string.Empty;

    /// <summary>
    /// The routing key to use when sending on <see cref="Exchange" />.
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;

    /// <summary>
    /// Whether the messages <b>MUST</b> route to a queue.
    /// </summary>
    public bool Mandatory { get; set; }

    /// <summary>
    /// Whether the exchange should survive a broker restart.
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Whether the exchange should be deleted when the channel is closed.
    /// </summary>
    public bool AutoDelete { get; set; } = false;

    /// <summary>
    /// Additional arguments for the exchange declaration.
    /// </summary>
    public Dictionary<string, object?> Arguments { get; set; } = [];

    /// <summary>
    /// Whether the exchange should be declared passive.
    /// </summary>
    public bool Passive { get; set; } = false;

    /// <summary>
    /// Whether a response from the server should be awaited.
    /// </summary>
    public bool NoWait { get; set; }
}