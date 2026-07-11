using API.Trainings.GetTraining;
using API.Trainings.Shared;
using Core.Trainings.Domain;
using IEventBus = Core.Shared.Events.IEventBus;
using Core.Trainings.Domain.Events;
using FastEndpoints;
using System.Security.Claims;

namespace API.Trainings.CreateTraining;

public sealed class CreateTrainingEndpoint : Endpoint<CreateTrainingRequest, TrainingResponse>
{
    private readonly ITrainingRepository _repository;
    private readonly IEventBus _eventBus;

    public CreateTrainingEndpoint(ITrainingRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public override void Configure()
    {
        Post("/trainings");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override async Task HandleAsync(CreateTrainingRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (req.DurationMinutes <= 0)
            AddError(r => r.DurationMinutes, "Duration must be positive.");

        ThrowIfAnyErrors();

        var training = Training.Create(userId, req.Date, req.DurationMinutes, req.Notes);

        if (req.Exercises.Count > 0)
        {
            var exerciseData = req.Exercises
                .Select(e => new ExerciseEntryData(
                    e.ExerciseName,
                    e.OrderIndex,
                    e.Sets
                        .Select(s => new ExerciseSetData(s.SetNumber, s.Reps, s.WeightKg, s.DurationSeconds, s.Notes))
                        .ToList()))
                .ToList();

            training.ReplaceExercises(exerciseData);
        }

        await _repository.AddAsync(training, ct);
        await _repository.SaveChangesAsync(ct);

        await _eventBus.PublishAsync(new TrainingCreatedEvent(training.Id, userId), ct);

        await SendCreatedAtAsync<GetTrainingEndpoint>(
            new { id = training.Id },
            training.ToResponse(),
            cancellation: ct);
    }
}
