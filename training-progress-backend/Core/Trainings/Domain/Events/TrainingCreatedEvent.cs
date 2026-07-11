namespace Core.Trainings.Domain.Events;

public sealed record TrainingCreatedEvent(Guid TrainingId, string UserId);
