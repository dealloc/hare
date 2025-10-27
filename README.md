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
- **Dead-letter queue support** - Automatic DLQ configuration and failed message routing
- **Distributed tracing** - Built-in OpenTelemetry support with correlation ID propagation
- **Aspire integration** - Works seamlessly with .NET Aspire for cloud-native development
- **Type-safe messaging** - Leverage generics for compile-time message type safety
- **Minimal dependencies** - Only depends on RabbitMQ.Client, OpenTelemetry, and Microsoft.Extensions abstractions

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

var builder = WebApplication.CreateBuilder(args);

// Add RabbitMQ connection
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory { HostName = "localhost" };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

// Or, if you're using .NET Aspire
builder.AddRabbitMQClient("rabbitmq");

// Register Hare with OpenTelemetry support and configure message type
builder.Services
    .AddHare()
    .AddHareMessage<OrderPlacedMessage>(config =>
    {
        config.Exchange = "orders";
        config.QueueName = "orders.placed";
        config.Durable = true;
        config.JsonTypeInfo = MessageSerializerContext.Default.OrderPlacedMessage;
    }, listen: false);

var app = builder.Build();

// Use the message sender
app.MapPost("/orders", async (
    OrderPlacedMessage message,
    IMessageSender<OrderPlacedMessage> sender,
    CancellationToken ct) =>
{
    await sender.SendMessageAsync(message, ct);
    return Results.Ok();
});

app.Run();
```

### 3. Consuming Messages

Configure and handle messages in your consumer application:

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

// Register Hare, configure message type, and register handler
builder.Services
    .AddHare()
    .AddHareMessage<OrderPlacedMessage>(config =>
    {
        config.Exchange = "orders";
        config.QueueName = "orders.placed";
        config.Durable = true;
        config.DeadletterExchange = "orders.dead-letter";
        config.DeadletterQueueName = "orders.placed.dead-letter";
        config.JsonTypeInfo = MessageSerializerContext.Default.OrderPlacedMessage;
    }, listen: true)
    .AddScoped<IMessageHandler<OrderPlacedMessage>, OrderPlacedHandler>();

var host = builder.Build();
host.Run();
```

### 4. Implement Message Handler

```csharp
public class OrderPlacedHandler(ILogger<OrderPlacedHandler> logger) : IMessageHandler<OrderPlacedMessage>
{
    public async ValueTask HandleAsync(OrderPlacedMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing order {OrderId} for customer {CustomerId}",
            message.OrderId, message.CustomerId);

        // Your business logic here
        await Task.CompletedTask;
    }
}
```

## Dead-Letter Queue Support

Hare automatically handles failed message processing with dead-letter queues:

1. **Automatic DLQ Setup** - Both producer and consumer create necessary exchanges and queues
2. **Requeue Logic** - Failed messages are requeued once, then routed to DLQ
3. **Configuration** - Both `DeadletterExchange` and `DeadletterQueueName` must be set together

```csharp
config.DeadletterExchange = "orders.dead-letter";
config.DeadletterQueueName = "orders.placed.dead-letter";
```

When a message handler throws an exception:
- **First failure**: Message is nacked and requeued
- **Second failure**: Message is sent to the dead-letter queue

## OpenTelemetry & Distributed Tracing

Hare includes built-in OpenTelemetry support with automatic correlation ID propagation:

```csharp
builder.Services.AddHare(); // Adds "Hare" ActivitySource

// The library automatically:
// - Sets correlation ID from Activity.Current?.Id when publishing
// - Creates linked activities when consuming messages
// - Traces message processing with proper parent-child relationships
```

This integrates seamlessly with .NET Aspire's dashboard for end-to-end tracing.

## AOT Compatibility

Hare is fully compatible with Native AOT compilation. To ensure AOT compatibility:

1. **Use JSON source generators** for serialization:

```csharp
[JsonSerializable(typeof(OrderPlacedMessage))]
public partial class MessageSerializerContext : JsonSerializerContext { }
```

2. **Provide `JsonTypeInfo`** when configuring messages:

```csharp
config.JsonTypeInfo = MessageSerializerContext.Default.OrderPlacedMessage;
```

3. **Use records for immutable messages** as shown in the examples above

## License

[MIT Licensed](./LICENSE.md)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.