namespace API.Trainings.UpdateTraining;

public sealed class UpdateTrainingRequest
{
    public Guid Id { get; init; }
    public DateOnly Date { get; init; }
    public int DurationMinutes { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyList<UpdateExerciseEntryRequest> Exercises { get; init; } = [];
}

public sealed record UpdateExerciseEntryRequest(
    string ExerciseName,
    int OrderIndex,
    IReadOnlyList<UpdateExerciseSetRequest> Sets);

public sealed record UpdateExerciseSetRequest(
    int SetNumber,
    int? Reps,
    decimal? WeightKg,
    int? DurationSeconds,
    string? Notes);
