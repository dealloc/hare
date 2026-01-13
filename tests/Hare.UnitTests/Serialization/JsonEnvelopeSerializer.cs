using Hare.Configuration;
using Hare.Contracts.Serialization;
using Hare.Infrastructure.Serialization;
using Hare.Models;

using Microsoft.Extensions.DependencyInjection;

using TUnit.Assertions.Enums;

namespace Hare.UnitTests.Serialization;

[Category("Unit")]
[Category("Serialization")]
public sealed class JsonEnvelopeSerializerTests
{
    [Test]
    public async Task Can_Serialize_Envelope(CancellationToken cancellationToken)
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<HareOptions>(static _ => { });
        services.AddSingleton<IEnvelopeSerializer, JsonEnvelopeSerializer>();
        var provider = services.BuildServiceProvider();

        var serializer = provider.GetRequiredService<IEnvelopeSerializer>();

        // Act
        var result = await serializer.SerializeAsync(new Envelope(
            MessageId: "3e629fcb-54b5-41ac-bcba-1feb9b76bdce",
            CorrelationId: "f51813d8-3ae0-4f66-b9b1-ec75508cefbb",
            MessageType: null,
            Timestamp: DateTimeOffset.Parse("2026-01-13T06:46:57.042693+00:00"),
            Payload: "{}"u8.ToArray()
        ), cancellationToken);

        // Assert
        await Assert.That(result)
            .IsEquivalentTo("{\"MessageId\":\"3e629fcb-54b5-41ac-bcba-1feb9b76bdce\",\"CorrelationId\":\"f51813d8-3ae0-4f66-b9b1-ec75508cefbb\",\"MessageType\":null,\"Timestamp\":\"2026-01-13T06:46:57.042693+00:00\",\"Payload\":\"e30=\"}"u8.ToArray(), CollectionOrdering.Matching);
    }

    [Test]
    public async Task Can_Deserialize_Envelope(CancellationToken cancellationToken)
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<HareOptions>(static _ => { });
        services.AddSingleton<IEnvelopeSerializer, JsonEnvelopeSerializer>();
        var provider = services.BuildServiceProvider();
        var expected = new Envelope(
            MessageId: "3e629fcb-54b5-41ac-bcba-1feb9b76bdce",
            CorrelationId: "f51813d8-3ae0-4f66-b9b1-ec75508cefbb",
            MessageType: null,
            Timestamp: DateTimeOffset.Parse("2026-01-13T06:46:57.042693+00:00"),
            Payload: "{}"u8.ToArray()
        );

        var serializer = provider.GetRequiredService<IEnvelopeSerializer>();

        // Act
        var result = await serializer.DeserializeAsync("{\"MessageId\":\"3e629fcb-54b5-41ac-bcba-1feb9b76bdce\",\"CorrelationId\":\"f51813d8-3ae0-4f66-b9b1-ec75508cefbb\",\"MessageType\":null,\"Timestamp\":\"2026-01-13T06:46:57.042693+00:00\",\"Payload\":\"e30=\"}"u8, cancellationToken);

        // Assert

        await Assert.That(result)
            .IsNotNull();
        await Assert.That(result.MessageId)
            .IsEqualTo(expected.MessageId);
        await Assert.That(result.CorrelationId)
            .IsEqualTo(expected.CorrelationId);
        await Assert.That(result.MessageType)
            .IsNull();
        await Assert.That(result.Timestamp)
            .IsEqualTo(expected.Timestamp);
        await Assert.That(result.Payload)
            .IsEquivalentTo(expected.Payload, CollectionOrdering.Matching);
    }
}