using System.Diagnostics;

using Hare.Configuration;
using Hare.Contracts.Transport;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hare.Infrastructure.Hosted;

/// <summary>
/// Manages launching and monitoring instances of <see cref="IListener{TMessage}" /> instances for <typeparamref name="TMessage" />.
/// </summary>
internal sealed class HostedListenerService<TMessage>(
    IOptionsMonitor<MessageOptions<TMessage>> options,
    ILogger<HostedListenerService<TMessage>> logger,
    IServiceScopeFactory scopeFactory
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var listenerCount = options.CurrentValue.ConcurrentListeners;
        logger.LogTrace("Starting {ListenerCount} listeners for {MessageType}", listenerCount, typeof(TMessage).Name);

        var workers = new Task[listenerCount];
        for (ulong i = 0; i < listenerCount; i++)
        {
            logger.LogTrace("Starting listener #{ListenerIndex} for {MessageType}", i, typeof(TMessage).Name);
            workers[i] = LaunchAndMonitorListener(cancellationToken);
        }

        await Task
            .WhenAll(workers)
            .WaitAsync(cancellationToken);
    }

    /// <summary>
    /// Launched for each listener: creates, starts and monitors the listener.
    /// </summary>
    private async Task LaunchAndMonitorListener(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var listener = scope.ServiceProvider.GetRequiredService<IListener<TMessage>>();

        try
        {
            await listener.ListenForIncomingMessagesAsync(cancellationToken);

            logger.LogWarning("Listener for {MessageType} stopped unexpectedly", typeof(TMessage).Name);
        }
        catch (Exception exception) when (cancellationToken.IsCancellationRequested is false)
        {
            Activity.Current?.AddException(exception);
            logger.LogCritical(exception, "Listener for {MessageType} failed", typeof(TMessage).Name);

            throw;
        }
    }
}