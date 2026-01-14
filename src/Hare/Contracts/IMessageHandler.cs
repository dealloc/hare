using Hare.Models;

namespace Hare.Contracts;

/// <summary>
/// Handles incoming messages of type <typeparamref name="TMessage"/>.
/// </summary>
public interface IMessageHandler<in TMessage>
{
    /// <summary>
    /// Handles an incoming <typeparamref name="TMessage" />.
    /// </summary>
    /// <param name="message">The message to handle.</param>
    /// <param name="context">Provides additional context about the incoming message.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    ValueTask HandleAsync(TMessage message, MessageContext context, CancellationToken cancellationToken);
}