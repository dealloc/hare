# Hare

A dead-simple, fast, and lightweight .NET messaging library for RabbitMQ.

## Overview

Hare is designed to be minimalistic and intentionally doesn't do a lot of things a fully-fledged service bus or messaging library would. Instead, it focuses on doing one thing well: simple, type-safe message publishing and consuming with RabbitMQ.

## Why Hare?

Hare is for you if:
- You are using **RabbitMQ** (the name `Hare` wouldn't make sense without **Rabbit**MQ anyway)
- You are using **System.Text.Json** for serialization
- You prefer **queue-per-type** / type-based routing patterns
- You want something simple without the overhead of full-featured service buses

## Features

- **Fully AOT compatible** - Works with [Native AOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/) compilation
- **Dead-letter queue support** - Automatic DLQ provisioning with conventional naming and configurable retry
- **Distributed tracing** - Built-in OpenTelemetry support with correlation ID propagation
- **Aspire integration** - Works seamlessly with .NET Aspire for cloud-native development
- **Type-safe messaging** - Leverage generics for compile-time message type safety
- **Minimal dependencies** - Only depends on RabbitMQ.Client and Microsoft.Extensions abstractions
- **Conventional routing** - Automatic exchange/queue naming based on message types
- **Auto-provisioning** - Automatic creation of exchanges, queues, and bindings

## Installation

```bash
dotnet add package Hare
```

## Quick Start

### 1. Define Your Message

```csharp
public record OrderPlacedMessage(
    string OrderId,
    string CustomerId,
    decimal Amount
);

// For AOT compatibility, use System.Text.Json source generators
[JsonSerializable(typeof(OrderPlacedMessage))]
public partial class MessageSerializerContext : JsonSerializerContext { }
```

### 2. Publishing Messages

Configure and send messages from your producer application:

```csharp
using Hare.Extensions;
using Hare.Contracts;

var builder = Host.CreateApplicationBuilder(args);

// Add RabbitMQ connection
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory { HostName = "localhost" };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

// Or, if you're using .NET Aspire
builder.AddRabbitMQClient("rabbitmq");

// Register Hare with the fluent builder API
builder.Services
    .AddHare()
    .WithConventionalRouting()               // Use default routing conventions
    .WithAutoProvisioning()                  // Automatically create exchanges/queues
    .WithJsonSerializerContext(MessageSerializerContext.Default)
    .AddHareMessage<OrderPlacedMessage>();   // Register message for sending

var host = builder.Build();

// Provision exchanges and queues before starting
await host.RunHareProvisioning(CancellationToken.None);

host.Run();
```

To send messages, inject `IMessageSender<TMessage>`:

```csharp
public class OrderService(IMessageSender<OrderPlacedMessage> sender)
{
    public async Task PlaceOrderAsync(Order order, CancellationToken ct)
    {
        var message = new OrderPlacedMessage(order.Id, order.CustomerId, order.Amount);
        await sender.SendAsync(message, ct);
    }
}
```

### 3. Consuming Messages

Configure and handle messages in your consumer application:

```csharp
using Hare.Extensions;
using Hare.Contracts;

var builder = Host.CreateApplicationBuilder(args);

// Add RabbitMQ connection (same as producer)
builder.AddRabbitMQClient("rabbitmq");

// Register Hare with message handler
builder.Services
    .AddHare()
    .WithConventionalRouting()
    .WithAutoProvisioning()
    .WithJsonSerializerContext(MessageSerializerContext.Default)
    .AddHareMessage<OrderPlacedMessage, OrderPlacedHandler>();  // Register with handler

var host = builder.Build();
await host.RunHareProvisioning(CancellationToken.None);
host.Run();
```

### 4. Implement Message Handler

```csharp
using Hare.Contracts;
using Hare.Models;

public class OrderPlacedHandler(ILogger<OrderPlacedHandler> logger) : IMessageHandler<OrderPlacedMessage>
{
    public ValueTask HandleAsync(
        OrderPlacedMessage message,
        MessageContext context,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing order {OrderId} for customer {CustomerId}",
            message.OrderId, message.CustomerId);

        // Access message metadata via context
        // context.Redelivered, context.Exchange, context.RoutingKey, context.Properties

        return ValueTask.CompletedTask;
    }
}
```

## Fluent Configuration API

Hare uses a fluent builder pattern for configuration:

### Global Configuration

```csharp
builder.Services
    .AddHare()
    .WithConventionalRouting()              // Enable default routing conventions
    .WithConventionalRouting<MyConvention>() // Or use custom routing convention
    .WithAutoProvisioning()                  // Auto-create exchanges/queues
    .WithJsonSerializerContext(context);     // Add JSON type info for AOT
```

### Per-Message Configuration

```csharp
builder.Services
    .AddHare()
    .WithConventionalRouting()
    .AddHareMessage<OrderMessage, OrderHandler>()
        .WithQueue("orders-queue")                   // Override queue name
        .WithExchange("orders", "direct")            // Override exchange
        .WithRoutingKey("orders.placed")             // Override routing key
        .WithConcurrency(4)                          // Number of concurrent listeners
        .WithDeadLetterExchange("orders.dlx")        // Override DLX name
        .WithDeadLetterRoutingKey("orders.failed")   // Override DLQ routing key
        .WithAutoProvisioning(false);                // Disable auto-provisioning for this message
```

## Conventional Routing

When `WithConventionalRouting()` is enabled, Hare automatically derives routing configuration from message type names:

- **Queue name**: Message type name in kebab-case (e.g., `OrderPlacedMessage` → `order-placed-message`)
- **Routing key**: Same as queue name
- **Exchange**: Entry assembly name in kebab-case
- **Exchange type**: `direct`
- **Dead-letter exchange**: `{exchange}.dlx`
- **Dead-letter queue**: `{queue}.dlq`

You can override any convention per-message using the fluent builder methods.

## Dead-Letter Queue Support

Hare provides built-in dead-letter queue (DLQ) support with automatic provisioning. Dead-lettering is **enabled by default** when using conventional routing.

### How It Works

- **First failure**: Message is nacked and requeued for retry
- **Second failure**: Message is nacked without requeue and routed to the dead-letter exchange

### Conventional DLQ Naming

When using `WithConventionalRouting()`, Hare automatically generates DLQ names:

- **Dead-letter exchange**: `{exchange-name}.dlx`
- **Dead-letter queue**: `{queue-name}.dlq`
- **Exchange type**: `direct`

For example, a message type `OrderPlacedMessage` in assembly `MyApp` would get:
- DLX: `my-app.dlx`
- DLQ: `order-placed-message.dlq`

### Custom DLQ Configuration

Override the conventional naming per-message:

```csharp
builder.Services
    .AddHare()
    .WithConventionalRouting()
    .AddHareMessage<OrderMessage, OrderHandler>()
        .WithDeadLetterExchange("orders.dlx", "direct")
        .WithDeadLetterRoutingKey("orders.failed");
```

### Disabling Dead-Lettering

To disable dead-lettering for a specific message type:

```csharp
.AddHareMessage<TransientMessage, TransientHandler>()
    .WithDeadLetter(false);
```

## OpenTelemetry & Distributed Tracing

Hare includes built-in OpenTelemetry support with automatic correlation ID propagation:

```csharp
// Add Hare's ActivitySource to your tracing configuration
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource("Hare.*"));

// The library automatically:
// - Sets correlation ID from Activity.Current?.Id when publishing
// - Creates linked activities when consuming messages
// - Traces message processing with proper parent-child relationships
```

This integrates seamlessly with .NET Aspire's dashboard for end-to-end tracing.

## Auto-Provisioning

When `WithAutoProvisioning()` is enabled, Hare automatically creates the required RabbitMQ resources before your application starts:

```csharp
var host = builder.Build();

// This creates exchanges, queues, and bindings
await host.RunHareProvisioning(CancellationToken.None);

host.Run();
```

You can enable/disable auto-provisioning globally or per-message type.

## AOT Compatibility

Hare is fully compatible with Native AOT compilation. To ensure AOT compatibility:

1. **Use JSON source generators** for serialization:

```csharp
[JsonSerializable(typeof(OrderPlacedMessage))]
[JsonSerializable(typeof(CustomerCreatedMessage))]
public partial class MessageSerializerContext : JsonSerializerContext { }
```

2. **Register the context** with Hare:

```csharp
builder.Services
    .AddHare()
    .WithJsonSerializerContext(MessageSerializerContext.Default);
```

3. **Use records** for immutable messages as shown in the examples above

## MessageContext

The `MessageContext` struct provides access to message metadata in your handlers:

```csharp
public readonly struct MessageContext
{
    public bool Redelivered { get; }           // Whether this is a redelivery
    public string Exchange { get; }            // Source exchange name
    public string RoutingKey { get; }          // Routing key used
    public IReadOnlyBasicProperties Properties { get; }  // RabbitMQ properties
    public ReadOnlyMemory<byte> Payload { get; }         // Raw message bytes
}
```

## License

[MIT Licensed](./LICENSE.md)

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](./CONTRIBUTING.md) for guidelines.
