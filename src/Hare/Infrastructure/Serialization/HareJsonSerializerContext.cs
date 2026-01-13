using System.Text.Json.Serialization;

using Hare.Models;

namespace Hare.Infrastructure.Serialization;

/// <summary>
/// Provides source-generated JSON type information for Hare models.
/// </summary>
[JsonSerializable(typeof(Envelope))]
internal sealed partial class HareJsonSerializerContext : JsonSerializerContext;