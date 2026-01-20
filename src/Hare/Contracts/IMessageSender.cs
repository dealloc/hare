using Hare.Models;

namespace Hare.Contracts;

/// <summary>
/// Sends messages of type <typeparamref name="TMessage"/>.
/// </summary>
public interface IMessageSender<TMessage>
{
    /// <summary>
    /// Attempts to send a <typeparamref name="TMessage" />.
    /// </summary>
    /// <returns>
    /// A unique identifier for the message sent, if one is available.
    /// </returns>
    ValueTask<string?> SendAsync(TMessage message, CancellationToken cancellationToken)
        => SendAsync(message, default, cancellationToken);

    /// <summary>
    /// Attempts to send a <typeparamref name="TMessage" />.
    /// </summary>
    /// <returns>
    /// A unique identifier for the message sent, if one is available.
    /// </returns>
    ValueTask<string?> SendAsync(TMessage message, MessageOptions options, CancellationToken cancellationToken);
}