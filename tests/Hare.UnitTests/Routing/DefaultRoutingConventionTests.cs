using Hare.Contracts.Routing;
using Hare.Routing;

using RabbitMQ.Client;

namespace Hare.UnitTests.Routing;

[Category("Unit")]
[Category("Routing")]
public sealed class DefaultRoutingConventionTests
{
    private readonly IRoutingConvention _convention = new DefaultRoutingConvention();

    [Test]
    [Arguments(typeof(ExampleMessage), "example-message")]
    [Arguments(typeof(OrderCreatedEvent), "order-created-event")]
    [Arguments(typeof(A), "a")]
    [Arguments(typeof(AB), "a-b")]
    public async Task GetQueueName_Converts_PascalCase_To_KebabCase(Type messageType, string expected)
    {
        // Act
        var result = _convention.GetQueueName(messageType);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task GetQueueName_Handles_SingleWord()
    {
        // Act
        var result = _convention.GetQueueName(typeof(Message));

        // Assert
        await Assert.That(result).IsEqualTo("message");
    }

    [Test]
    public async Task GetQueueName_Handles_Acronyms_As_Separate_Letters()
    {
        // Note: Current implementation treats each capital as a separate word
        // HTTPRequest → h-t-t-p-request
        // Act
        var result = _convention.GetQueueName(typeof(HTTPRequest));

        // Assert
        await Assert.That(result).IsEqualTo("h-t-t-p-request");
    }

    [Test]
    public async Task GetQueueName_Handles_TrailingAcronym()
    {
        // Act
        var result = _convention.GetQueueName(typeof(SendHTTP));

        // Assert
        await Assert.That(result).IsEqualTo("send-h-t-t-p");
    }

    [Test]
    public async Task GetQueueName_Preserves_Numbers()
    {
        // Act
        var result = _convention.GetQueueName(typeof(Order123Event));

        // Assert
        await Assert.That(result).IsEqualTo("order123-event");
    }

    [Test]
    public async Task GetQueueName_Handles_Lowercase_Input()
    {
        // Act
        var result = _convention.GetQueueName(typeof(lowercase));

        // Assert
        await Assert.That(result).IsEqualTo("lowercase");
    }

    [Test]
    public async Task GetRoutingKey_Returns_Same_As_QueueName()
    {
        // Act
        var queueName = _convention.GetQueueName(typeof(ExampleMessage));
        var routingKey = _convention.GetRoutingKey(typeof(ExampleMessage));

        // Assert
        await Assert.That(routingKey).IsEqualTo(queueName);
    }

    [Test]
    public async Task GetExchange_Returns_Assembly_Name()
    {
        // Act
        var result = _convention.GetExchange(typeof(ExampleMessage));

        // Assert
        await Assert.That(result).IsEqualTo("hare-unit-tests");
    }

    [Test]
    public async Task GetExchangeType_Returns_Direct()
    {
        // Act
        var result = _convention.GetExchangeType(typeof(ExampleMessage));

        // Assert
        await Assert.That(result).IsEqualTo(ExchangeType.Direct);
    }

    // Test types
    internal sealed class ExampleMessage;
    internal sealed class OrderCreatedEvent;
    internal sealed class Message;
    internal sealed class A;
    internal sealed class AB;
    internal sealed class HTTPRequest;
    internal sealed class SendHTTP;
    internal sealed class Order123Event;
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
    internal sealed class lowercase;
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
}