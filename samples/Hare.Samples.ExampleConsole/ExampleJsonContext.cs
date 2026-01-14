using System.Text.Json.Serialization;

using Hare.Samples.ExampleConsole.Messages;

namespace Hare.Samples.ExampleConsole;

[JsonSerializable(typeof(ExampleMessage))]
public sealed partial class ExampleJsonContext : JsonSerializerContext;