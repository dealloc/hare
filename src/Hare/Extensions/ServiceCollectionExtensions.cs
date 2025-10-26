using Hare.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddHare(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddSource("Hare"));
    }

    public static void AddHareMessage<TMessage>(this IServiceCollection services, Action<HareMessageConfiguration<TMessage>>? configure = null)
    {
        services.AddOptions<HareMessageConfiguration<TMessage>>();
    }
}