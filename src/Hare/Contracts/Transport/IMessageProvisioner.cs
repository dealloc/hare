namespace Hare.Contracts.Transport;

/// <summary>
/// Handles provisioning exchanges, queues, and bindings for a message as required.
/// </summary>
/// <remarks>
/// The provisioner is <b>ALWAYS</b> invoked on provisioning,
/// implementations are expected to check if provisioning is required.
/// </remarks>
public interface IMessageProvisioner
{
    /// <summary>
    /// Execute the provisioning logic required for this message.
    /// </summary>
    ValueTask ProvisionAsync(CancellationToken cancellationToken = default);
}