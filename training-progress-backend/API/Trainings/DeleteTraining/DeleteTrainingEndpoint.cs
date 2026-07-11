using Core.Trainings.Domain;
using IEventBus = Core.Shared.Events.IEventBus;
using Core.Trainings.Domain.Events;
using FastEndpoints;
using System.Security.Claims;

namespace API.Trainings.DeleteTraining;

public sealed class DeleteTrainingEndpoint : EndpointWithoutRequest
{
    private readonly ITrainingRepository _repository;
    private readonly IEventBus _eventBus;

    public DeleteTrainingEndpoint(ITrainingRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public override void Configure()
    {
        Delete("/trainings/{id}");
        // TODO: Remove AllowAnonymous and configure Clerk JWT bearer authentication
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var id = Route<Guid>("id");

        var training = await _repository.GetByIdAsync(id, userId, ct);
        if (training is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        _repository.Remove(training);
        await _repository.SaveChangesAsync(ct);

        await _eventBus.PublishAsync(new TrainingDeletedEvent(id, userId), ct);

        await SendNoContentAsync(ct);
    }
}
