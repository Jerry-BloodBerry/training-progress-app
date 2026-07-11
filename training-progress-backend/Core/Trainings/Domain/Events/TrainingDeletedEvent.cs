namespace Core.Trainings.Domain.Events;

public sealed record TrainingDeletedEvent(Guid TrainingId, string UserId);
