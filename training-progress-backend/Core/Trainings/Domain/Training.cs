namespace Core.Trainings.Domain;

public sealed class Training
{
    private readonly List<ExerciseEntry> _exerciseEntries = [];

    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public DateOnly Date { get; private set; }
    public int DurationMinutes { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public TrainingStatistics Statistics { get; private set; } = TrainingStatistics.Empty;

    public IReadOnlyList<ExerciseEntry> ExerciseEntries => _exerciseEntries.AsReadOnly();

    private Training() { }

    public static Training Create(string userId, DateOnly date, int durationMinutes, string? notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        if (durationMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes), "Duration must be positive.");

        var now = DateTimeOffset.UtcNow;
        return new Training
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = date,
            DurationMinutes = durationMinutes,
            Notes = notes,
            CreatedAt = now,
            UpdatedAt = now,
            Statistics = TrainingStatistics.Empty,
        };
    }

    public void Update(DateOnly date, int durationMinutes, string? notes)
    {
        if (durationMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes), "Duration must be positive.");

        Date = date;
        DurationMinutes = durationMinutes;
        Notes = notes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ReplaceExercises(IReadOnlyList<ExerciseEntryData> exercises)
    {
        _exerciseEntries.Clear();

        foreach (var entry in exercises)
        {
            var exerciseEntry = ExerciseEntry.Create(Id, entry.ExerciseName, entry.OrderIndex);
            foreach (var set in entry.Sets)
            {
                exerciseEntry.AddSet(set.SetNumber, set.Reps, set.WeightKg, set.DurationSeconds, set.Notes);
            }
            _exerciseEntries.Add(exerciseEntry);
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecalculateStatistics()
    {
        var allSets = _exerciseEntries.SelectMany(e => e.Sets).ToList();
        var totalSets = allSets.Count;
        var totalReps = allSets.Sum(s => s.Reps ?? 0);
        var totalVolumeKg = allSets.Sum(s => (s.WeightKg ?? 0m) * (s.Reps ?? 0));

        Statistics = new TrainingStatistics(totalVolumeKg, totalSets, totalReps, null);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
