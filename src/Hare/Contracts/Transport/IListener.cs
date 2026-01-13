namespace Hare.Contracts.Transport;

/// <summary>
/// Listens for incoming <typeparamref name="TMessage" /> on the transport layer, more than one instance may be started.
/// </summary>
public interface IListener<TMessage>
{
    /// <summary>
    /// Start listening for incoming <typeparamref name="TMessage" />s.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
    /// <remarks>
    /// This method is expected to block until <paramref name="cancellationToken" /> is cancelled.
    /// If this method returns prematurely, it will <b>NOT</b> be restarted.
    /// </remarks>
    /// <exception cref="Exception">
    /// If an error occurs during the listening process.
    /// Throwing from this method should be considered critical and may cause the application to crash.
    /// </exception>
    Task ListenForIncomingMessagesAsync(CancellationToken cancellationToken);
}