using System.Diagnostics;

using Hare.Contracts.Transport;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hare.Extensions;

/// <summary>
/// Contains extension methods for <see cref="IHost"/>.
/// </summary>
public static class HostExtensions
{
    /// <summary>
    /// Runs all provisioning for registered messages.
    /// </summary>
    public static async Task RunHareProvisioning(this IHost host, CancellationToken cancellationToken)
    {
        var source = new ActivitySource($"{Constants.ACTIVITY_PREFIX}");
        await using var scope = host.Services.CreateAsyncScope();
        var provisioners = scope.ServiceProvider.GetServices<IMessageProvisioner>();

        foreach (var provisioner in provisioners)
        {
            using var activity = source.StartActivity($"{provisioner.GetType().FullName}");

            try
            {
                await provisioner.ProvisionAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                activity?.AddException(exception);
                throw;
            }
        }
    }
}