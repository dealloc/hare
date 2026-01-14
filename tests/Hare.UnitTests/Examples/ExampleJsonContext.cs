using System.Text.Json.Serialization;

namespace Hare.UnitTests.Examples;

[JsonSerializable(typeof(SimpleExampleMessage))]
[JsonSerializable(typeof(RecordExampleMessage))]
[JsonSerializable(typeof(EmptyExampleMessage))]
public sealed partial class ExampleJsonContext : JsonSerializerContext;