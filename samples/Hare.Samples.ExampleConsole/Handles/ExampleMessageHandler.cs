using Hare.Contracts;
using Hare.Models;
using Hare.Samples.ExampleConsole.Messages;

namespace Hare.Samples.ExampleConsole.Handles;

public sealed class ExampleMessageHandler : IMessageHandler<ExampleMessage>
{
    /// <inheritdoc />
    public ValueTask HandleAsync(ExampleMessage message, MessageContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}