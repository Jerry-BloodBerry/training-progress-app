using API.Trainings.Shared;
using Core.Trainings.Domain;
using IEventBus = Core.Shared.Events.IEventBus;
using Core.Trainings.Domain.Events;
using FastEndpoints;
using System.Security.Claims;

namespace API.Trainings.UpdateTraining;

public sealed class UpdateTrainingEndpoint : Endpoint<UpdateTrainingRequest, TrainingResponse>
{
    private readonly ITrainingRepository _repository;
    private readonly IEventBus _eventBus;

    public UpdateTrainingEndpoint(ITrainingRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public override void Configure()
    {
        Put("/trainings/{id}");
        // TODO: Remove AllowAnonymous and configure Clerk JWT bearer authentication
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateTrainingRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        if (req.DurationMinutes <= 0)
            AddError(r => r.DurationMinutes, "Duration must be positive.");

        ThrowIfAnyErrors();

        var training = await _repository.GetByIdAsync(req.Id, userId, ct);
        if (training is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        training.Update(req.Date, req.DurationMinutes, req.Notes);

        var exerciseData = req.Exercises
            .Select(e => new ExerciseEntryData(
                e.ExerciseName,
                e.OrderIndex,
                e.Sets
                    .Select(s => new ExerciseSetData(s.SetNumber, s.Reps, s.WeightKg, s.DurationSeconds, s.Notes))
                    .ToList()))
            .ToList();

        training.ReplaceExercises(exerciseData);

        _repository.Update(training);
        await _repository.SaveChangesAsync(ct);

        await _eventBus.PublishAsync(new TrainingUpdatedEvent(training.Id, userId), ct);

        await SendOkAsync(training.ToResponse(), ct);
    }
}
