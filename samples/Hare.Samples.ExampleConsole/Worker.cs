using Hare.Contracts;
using Hare.Samples.ExampleConsole.Messages;

namespace Hare.Samples.ExampleConsole;

public class Worker(
    ILogger<Worker> logger,
    IMessageSender<ExampleMessage> messageSender
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await messageSender.SendAsync(new ExampleMessage("Hello World"), stoppingToken);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}