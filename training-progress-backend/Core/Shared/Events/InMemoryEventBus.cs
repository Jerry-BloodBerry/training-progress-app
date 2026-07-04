using Microsoft.Extensions.DependencyInjection;

namespace Core.Shared.Events;

internal sealed class InMemoryEventBus : IEventBus
{
    private readonly IServiceProvider _sp;

    public InMemoryEventBus(IServiceProvider sp) => _sp = sp;

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class
    {
        var handlers = _sp.GetServices<IEventHandler<TEvent>>();
        foreach (var handler in handlers)
        {
            await handler.HandleAsync(@event, ct);
        }
    }
}
