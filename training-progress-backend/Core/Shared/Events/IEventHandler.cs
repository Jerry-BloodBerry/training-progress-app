namespace Core.Shared.Events;

public interface IEventHandler<TEvent>
    where TEvent : class
{
    Task HandleAsync(TEvent @event, CancellationToken ct);
}
