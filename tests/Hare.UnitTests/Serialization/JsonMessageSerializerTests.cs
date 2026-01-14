using Hare.Configuration;
using Hare.Contracts.Serialization;
using Hare.Infrastructure.Serialization;
using Hare.UnitTests.Examples;

using Microsoft.Extensions.DependencyInjection;

using TUnit.Assertions.Enums;

namespace Hare.UnitTests.Serialization;

[Category("Unit")]
[Category("Serialization")]
public sealed class JsonMessageSerializerTests
{
    [Test]
    public async Task Can_Serialize_SimpleExampleMessage(CancellationToken cancellationToken)
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<MessageOptions<SimpleExampleMessage>>(options =>
            options.JsonSerializerOptions = new()
            {
                TypeInfoResolverChain = { ExampleJsonContext.Default }
            }
        );
        services.AddSingleton<IMessageSerializer<SimpleExampleMessage>, JsonMessageSerializer<SimpleExampleMessage>>();
        var provider = services.BuildServiceProvider();

        var serializer = provider.GetRequiredService<IMessageSerializer<SimpleExampleMessage>>();

        // Act
        var result = await serializer.SerializeAsync(new SimpleExampleMessage { Text = "Test Message" }, cancellationToken);

        // Assert
        await Assert.That(result)
            .IsEquivalentTo("{\"Text\":\"Test Message\"}"u8.ToArray(), CollectionOrdering.Matching);
    }

    [Test]
    public async Task Can_Serialize_RecordExampleMessage(CancellationToken cancellationToken)
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<MessageOptions<RecordExampleMessage>>(options =>
            options.JsonSerializerOptions = new()
            {
                TypeInfoResolverChain = { ExampleJsonContext.Default }
            }
        );
        services.AddSingleton<IMessageSerializer<RecordExampleMessage>, JsonMessageSerializer<RecordExampleMessage>>();
        var provider = services.BuildServiceProvider();

        var serializer = provider.GetRequiredService<IMessageSerializer<RecordExampleMessage>>();

        // Act
        var result = await serializer.SerializeAsync(new RecordExampleMessage("Test Message"), cancellationToken);

        // Assert
        await Assert.That(result)
            .IsEquivalentTo("{\"Text\":\"Test Message\"}"u8.ToArray(), CollectionOrdering.Matching);
    }

    [Test]
    public async Task Can_Serialize_EmptyExampleMessage(CancellationToken cancellationToken)
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<MessageOptions<EmptyExampleMessage>>(options =>
            options.JsonSerializerOptions = new()
            {
                TypeInfoResolverChain = { ExampleJsonContext.Default }
            }
        );
        services.AddSingleton<IMessageSerializer<EmptyExampleMessage>, JsonMessageSerializer<EmptyExampleMessage>>();
        var provider = services.BuildServiceProvider();

        var serializer = provider.GetRequiredService<IMessageSerializer<EmptyExampleMessage>>();

        // Act
        var result = await serializer.SerializeAsync(new EmptyExampleMessage(), cancellationToken);

        // Assert
        await Assert.That(result)
            .IsEquivalentTo("{}"u8.ToArray(), CollectionOrdering.Matching);
    }

    [Test]
    public async Task Can_DeSerialize_SimpleExampleMessage(CancellationToken cancellationToken)
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<MessageOptions<SimpleExampleMessage>>(options =>
            options.JsonSerializerOptions = new()
            {
                TypeInfoResolverChain = { ExampleJsonContext.Default }
            }
        );
        services.AddSingleton<IMessageSerializer<SimpleExampleMessage>, JsonMessageSerializer<SimpleExampleMessage>>();
        var provider = services.BuildServiceProvider();

        var serializer = provider.GetRequiredService<IMessageSerializer<SimpleExampleMessage>>();

        // Act
        var result = await serializer.DeserializeAsync("{\"Text\":\"Test Message\"}"u8.ToArray(), cancellationToken);

        // Assert
        await Assert.That(result)
            .IsNotNull();
        await Assert.That(result.Text)
            .IsEqualTo("Test Message");
    }

    [Test]
    public async Task Can_DeSerialize_RecordExampleMessage(CancellationToken cancellationToken)
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<MessageOptions<RecordExampleMessage>>(options =>
            options.JsonSerializerOptions = new()
            {
                TypeInfoResolverChain = { ExampleJsonContext.Default }
            }
        );
        services.AddSingleton<IMessageSerializer<RecordExampleMessage>, JsonMessageSerializer<RecordExampleMessage>>();
        var provider = services.BuildServiceProvider();

        var serializer = provider.GetRequiredService<IMessageSerializer<RecordExampleMessage>>();

        // Act
        var result = await serializer.DeserializeAsync("{\"Text\":\"Test Message\"}"u8.ToArray(), cancellationToken);

        // Assert
        await Assert.That(result)
            .IsNotNull();
        await Assert.That(result.Text)
            .IsEqualTo("Test Message");
    }

    [Test]
    public async Task Can_DeSerialize_EmptyExampleMessage(CancellationToken cancellationToken)
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<MessageOptions<EmptyExampleMessage>>(options =>
            options.JsonSerializerOptions = new()
            {
                TypeInfoResolverChain = { ExampleJsonContext.Default }
            }
        );
        services.AddSingleton<IMessageSerializer<EmptyExampleMessage>, JsonMessageSerializer<EmptyExampleMessage>>();
        var provider = services.BuildServiceProvider();

        var serializer = provider.GetRequiredService<IMessageSerializer<EmptyExampleMessage>>();

        // Act
        var result = await serializer.DeserializeAsync("{}"u8.ToArray(), cancellationToken);

        // Assert
        await Assert.That(result)
            .IsNotNull();
    }

    [Test]
    public async Task Cannot_Serialize_With_Reflection_On_AOT(CancellationToken cancellationToken)
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<MessageOptions<EmptyExampleMessage>>(static _ => { });
        services.AddSingleton<IMessageSerializer<EmptyExampleMessage>, JsonMessageSerializer<EmptyExampleMessage>>();
        var provider = services.BuildServiceProvider();

        var serializer = provider.GetRequiredService<IMessageSerializer<EmptyExampleMessage>>();

        // Act

        // Assert
        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await serializer.DeserializeAsync("{}"u8.ToArray(), cancellationToken));
        await Assert.That(exception?.Message)
            .StartsWith("Reflection-based serialization has been disabled for this application");
    }

    [Test]
    public async Task Can_Use_Global_JsonTypeInformation(CancellationToken cancellationToken)
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<HareOptions>(options =>
            options.JsonSerializerOptions = new()
            {
                TypeInfoResolverChain = { ExampleJsonContext.Default }
            }
        );
        services.AddSingleton<IMessageSerializer<SimpleExampleMessage>, JsonMessageSerializer<SimpleExampleMessage>>();
        var provider = services.BuildServiceProvider();

        var serializer = provider.GetRequiredService<IMessageSerializer<SimpleExampleMessage>>();

        // Act
        var result = await serializer.SerializeAsync(new SimpleExampleMessage { Text = "Test Message" }, cancellationToken);

        // Assert
        await Assert.That(result)
            .IsEquivalentTo("{\"Text\":\"Test Message\"}"u8.ToArray(), CollectionOrdering.Matching);
    }
}