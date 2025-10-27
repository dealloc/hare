using System.Text.Json.Serialization.Metadata;

namespace Hare.Configuration;

public sealed class HareMessageConfiguration<TMessage>
{
    public string Exchange { get; set; } = string.Empty;

    public string? DeadletterExchange { get; set; } = $"{typeof(TMessage).FullName ?? typeof(TMessage).Name}.dead-letter";

    public string QueueName { get; set; } = typeof(TMessage).FullName ?? typeof(TMessage).Name;

    public string? DeadletterQueueName { get; set; } = $"{typeof(TMessage).FullName ?? typeof(TMessage).Name}.dead-letter";

    public bool Durable { get; set; } = false;

    public bool Exclusive { get; set; } = false;

    public bool AutoDelete { get; set; } = false;

    public IDictionary<string, object?>? Arguments { get; set; }

    public required JsonTypeInfo<TMessage> JsonTypeInfo { get; set; }
}