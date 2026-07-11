namespace Core.Trainings.Domain;

public sealed class ExerciseSet
{
    public Guid Id { get; private set; }
    public Guid ExerciseEntryId { get; private set; }
    public int SetNumber { get; private set; }
    public int? Reps { get; private set; }
    public decimal? WeightKg { get; private set; }
    public int? DurationSeconds { get; private set; }
    public string? Notes { get; private set; }

    private ExerciseSet() { }

    internal static ExerciseSet Create(
        Guid exerciseEntryId,
        int setNumber,
        int? reps,
        decimal? weightKg,
        int? durationSeconds,
        string? notes)
    {
        return new ExerciseSet
        {
            Id = Guid.NewGuid(),
            ExerciseEntryId = exerciseEntryId,
            SetNumber = setNumber,
            Reps = reps,
            WeightKg = weightKg,
            DurationSeconds = durationSeconds,
            Notes = notes,
        };
    }
}
