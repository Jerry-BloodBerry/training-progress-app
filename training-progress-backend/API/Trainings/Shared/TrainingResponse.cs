namespace API.Trainings.Shared;

public sealed record TrainingResponse(
    Guid Id,
    DateOnly Date,
    int DurationMinutes,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ExerciseEntryResponse> Exercises,
    TrainingStatisticsResponse Statistics);

public sealed record ExerciseEntryResponse(
    Guid Id,
    string ExerciseName,
    int OrderIndex,
    IReadOnlyList<ExerciseSetResponse> Sets);

public sealed record ExerciseSetResponse(
    Guid Id,
    int SetNumber,
    int? Reps,
    decimal? WeightKg,
    int? DurationSeconds,
    string? Notes);

public sealed record TrainingStatisticsResponse(
    decimal TotalVolumeKg,
    int TotalSets,
    int TotalReps,
    int? EstimatedCalories);
