namespace Core.Trainings.Domain;

public sealed record ExerciseEntryData(
    string ExerciseName,
    int OrderIndex,
    IReadOnlyList<ExerciseSetData> Sets);

public sealed record ExerciseSetData(
    int SetNumber,
    int? Reps,
    decimal? WeightKg,
    int? DurationSeconds,
    string? Notes);
