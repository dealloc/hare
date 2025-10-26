using System.Text.Json.Serialization.Metadata;

namespace Hare.Configuration;

public sealed class HareConfiguration
{
    public List<IJsonTypeInfoResolver> TypeInfoResolverChain { get; set; } = [];
}