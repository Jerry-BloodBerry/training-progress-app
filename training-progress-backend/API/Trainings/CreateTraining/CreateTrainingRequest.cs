namespace API.Trainings.CreateTraining;

public sealed record CreateTrainingRequest(
    DateOnly Date,
    int DurationMinutes,
    string? Notes,
    IReadOnlyList<CreateExerciseEntryRequest> Exercises);

public sealed record CreateExerciseEntryRequest(
    string ExerciseName,
    int OrderIndex,
    IReadOnlyList<CreateExerciseSetRequest> Sets);

public sealed record CreateExerciseSetRequest(
    int SetNumber,
    int? Reps,
    decimal? WeightKg,
    int? DurationSeconds,
    string? Notes);
