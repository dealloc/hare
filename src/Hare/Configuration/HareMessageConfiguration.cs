using System.Text.Json.Serialization.Metadata;

namespace Hare.Configuration;

/// <summary>
/// Configures how Hare should handle sending and receiving messages of type <typeparamref name="TMessage" />.
/// </summary>
public sealed class HareMessageConfiguration<TMessage> where TMessage : class
{
    /// <summary>
    /// The name of the RabbitMQ exchange for sending and receiving messages.
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// The name of the RabbitMQ exchange for deadlettering messages.
    ///
    /// Defaults to the full name of <typeparamref name="TMessage" /> suffixed with <c>.dead-letter</c>.
    /// </summary>
    /// <remarks>If left empty / <c>null</c>, deadlettering is disabled.</remarks>
    public string? DeadLetterExchange { get; set; } = $"{typeof(TMessage).FullName ?? typeof(TMessage).Name}.dead-letter";

    /// <summary>
    /// The name of the RabbitMQ queue for sending / receiving messages.
    /// </summary>
    public string QueueName { get; set; } = typeof(TMessage).FullName ?? typeof(TMessage).Name;

    /// <summary>
    /// The name of the RabbitMQ queue of deadlettering messages.
    ///
    /// Defaults to the full name of <typeparamref name="TMessage" /> suffixed with <c>.dead-letter</c>.
    /// </summary>
    /// <remarks>If left empty / <c>null</c>, deadlettering is disabled.</remarks>
    public string? DeadLetterQueueName { get; set; } = $"{typeof(TMessage).FullName ?? typeof(TMessage).Name}.dead-letter";

    /// <summary>
    /// The <c>durable</c> flag for RabbitMQ.
    /// </summary>
    public bool Durable { get; set; } = false;

    /// <summary>
    /// The <c>exclusive</c> flag for RabbitMQ.
    /// </summary>
    public bool Exclusive { get; set; } = false;

    /// <summary>
    /// The <c>autodelete</c> flag for RabbitMQ.
    /// </summary>
    public bool AutoDelete { get; set; } = false;

    /// <summary>
    /// Additional arguments to pass to the queue when declaring.
    /// </summary>
    public IDictionary<string, object?>? Arguments { get; set; }

    /// <summary>
    /// The type information used when (de)serializing messages for/from RabbitMQ.
    /// </summary>
    public required JsonTypeInfo<TMessage> JsonTypeInfo { get; set; }
}