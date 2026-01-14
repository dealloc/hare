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
            logger.LogInformation("Worker sending message: {time}", DateTimeOffset.Now);
            await messageSender.SendAsync(new ExampleMessage($"{DateTimeOffset.Now}"), stoppingToken);

            await Task.Delay(1000, stoppingToken);
        }
    }
}