using System.Reflection;
using System.Text;

using Hare.Contracts.Routing;

using RabbitMQ.Client;

namespace Hare.Routing;

/// <summary>
/// Default routing convention that derives queue and routing names from message type names using kebab-case.
/// </summary>
/// <remarks>
/// <p>
/// This convention uses the default exchange (empty string) which routes directly to queues by name.
/// </p>
/// <p>
/// Example: <c>ExampleMessage</c> becomes queue name <c>example-message</c>.
/// </p>
/// </remarks>
public class DefaultRoutingConvention : IRoutingConvention
{
    /// <inheritdoc />
    public virtual string GetQueueName(Type messageType)
        => ToKebabCase(messageType.Name);

    /// <inheritdoc />
    public virtual string GetRoutingKey(Type messageType)
        => GetQueueName(messageType);

    /// <inheritdoc />
    public string GetDeadLetterRoutingKey(Type messageType)
        => $"{GetQueueName(messageType)}.dlq";

    /// <inheritdoc />
    public virtual string GetExchange(Type messageType)
        => ToKebabCase(Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty);

    /// <inheritdoc />
    public string GetDeadLetterExchange(Type messageType)
        => $"{GetExchange(messageType)}.dlx";

    /// <inheritdoc />
    public virtual string GetExchangeType(Type messageType)
        => ExchangeType.Direct;

    /// <inheritdoc />
    public string GetDeadLetterExchangeType(Type messageType)
        => ExchangeType.Direct;

    /// <summary>
    /// Converts a name to kebab-case.
    /// </summary>
    /// <param name="name">The name to convert.</param>
    protected static string ToKebabCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;

        var builder = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (c == '.')
                continue;
            if (char.IsUpper(c))
            {
                if (i > 0)
                    builder.Append('-');
                builder.Append(char.ToLowerInvariant(c));
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}