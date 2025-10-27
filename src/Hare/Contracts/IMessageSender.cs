namespace Hare.Contracts;

public interface IMessageSender<in TMessage> where TMessage : class
{
    public ValueTask SendMessageAsync(TMessage message, CancellationToken cancellationToken);
}