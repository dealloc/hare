namespace Hare.Contracts;

public interface IMessageHandler<in TMessage>
{
    public ValueTask HandleAsync(TMessage message, CancellationToken cancellationToken);
}