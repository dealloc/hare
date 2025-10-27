using Hare.Configuration;
using Hare.Contracts;
using Hare.Hosted;
using Hare.Services;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHare(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddSource("Hare"));

        return services;
    }

    public static IServiceCollection AddHareMessage<TMessage>(this IServiceCollection services,
        Action<HareMessageConfiguration<TMessage>>? configure = null) where TMessage : class
    {
        services.AddOptions<HareMessageConfiguration<TMessage>>();
        if (configure is not null)
            services.Configure<HareMessageConfiguration<TMessage>>(configure);

        services.AddHostedService<MessageReceiverService<TMessage>>();
        services.AddScoped<IMessageSender<TMessage>, DefaultMessageSender<TMessage>>();
        return services;
    }
}