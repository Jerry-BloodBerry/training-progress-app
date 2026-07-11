namespace Core.Trainings.Domain.Events;

public sealed record TrainingUpdatedEvent(Guid TrainingId, string UserId);
