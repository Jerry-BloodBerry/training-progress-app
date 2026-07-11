namespace Core.Trainings.Domain;

public sealed class ExerciseEntry
{
    private readonly List<ExerciseSet> _sets = [];

    public Guid Id { get; private set; }
    public Guid TrainingId { get; private set; }
    public string ExerciseName { get; private set; } = string.Empty;
    public int OrderIndex { get; private set; }

    public IReadOnlyList<ExerciseSet> Sets => _sets.AsReadOnly();

    private ExerciseEntry() { }

    internal static ExerciseEntry Create(Guid trainingId, string exerciseName, int orderIndex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exerciseName);

        return new ExerciseEntry
        {
            Id = Guid.NewGuid(),
            TrainingId = trainingId,
            ExerciseName = exerciseName,
            OrderIndex = orderIndex,
        };
    }

    internal void AddSet(int setNumber, int? reps, decimal? weightKg, int? durationSeconds, string? notes)
    {
        _sets.Add(ExerciseSet.Create(Id, setNumber, reps, weightKg, durationSeconds, notes));
    }
}
