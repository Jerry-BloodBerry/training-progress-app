using Core.Shared.Events;
using Core.Trainings.Domain;
using Core.Trainings.Domain.Events;

namespace Core.Trainings.Application;

internal sealed class TrainingStatisticsHandler :
    IEventHandler<TrainingCreatedEvent>,
    IEventHandler<TrainingUpdatedEvent>
{
    private readonly ITrainingRepository _repository;

    public TrainingStatisticsHandler(ITrainingRepository repository) => _repository = repository;

    public Task HandleAsync(TrainingCreatedEvent @event, CancellationToken ct) =>
        UpdateStatisticsAsync(@event.TrainingId, @event.UserId, ct);

    public Task HandleAsync(TrainingUpdatedEvent @event, CancellationToken ct) =>
        UpdateStatisticsAsync(@event.TrainingId, @event.UserId, ct);

    private async Task UpdateStatisticsAsync(Guid trainingId, string userId, CancellationToken ct)
    {
        var training = await _repository.GetByIdAsync(trainingId, userId, ct);
        if (training is null) return;

        training.RecalculateStatistics();
        await _repository.SaveChangesAsync(ct);
    }
}
