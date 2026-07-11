namespace Core.Trainings.Domain;

public sealed class TrainingStatistics
{
    public static readonly TrainingStatistics Empty = new(0m, 0, 0, null);

    public decimal TotalVolumeKg { get; init; }
    public int TotalSets { get; init; }
    public int TotalReps { get; init; }
    public int? EstimatedCalories { get; init; }

    // Required by EF Core JSON / System.Text.Json deserialization
    public TrainingStatistics() { }

    public TrainingStatistics(decimal totalVolumeKg, int totalSets, int totalReps, int? estimatedCalories)
    {
        TotalVolumeKg = totalVolumeKg;
        TotalSets = totalSets;
        TotalReps = totalReps;
        EstimatedCalories = estimatedCalories;
    }
}
