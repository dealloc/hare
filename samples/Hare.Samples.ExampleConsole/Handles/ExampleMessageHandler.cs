using Hare.Contracts;
using Hare.Models;
using Hare.Samples.ExampleConsole.Messages;

namespace Hare.Samples.ExampleConsole.Handles;

public sealed class ExampleMessageHandler(ILogger<ExampleMessageHandler> logger) : IMessageHandler<ExampleMessage>
{
    /// <inheritdoc />
    public ValueTask HandleAsync(ExampleMessage message, MessageContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received message: {Text}", message.Text);

        return ValueTask.CompletedTask;
    }
}