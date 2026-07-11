using API.Trainings.Shared;
using Core.Trainings.Domain;

namespace API.Trainings;

internal static class TrainingMapper
{
    internal static TrainingResponse ToResponse(this Training training) =>
        new(
            training.Id,
            training.Date,
            training.DurationMinutes,
            training.Notes,
            training.CreatedAt,
            training.UpdatedAt,
            training.ExerciseEntries
                .OrderBy(e => e.OrderIndex)
                .Select(e => new ExerciseEntryResponse(
                    e.Id,
                    e.ExerciseName,
                    e.OrderIndex,
                    e.Sets
                        .OrderBy(s => s.SetNumber)
                        .Select(s => new ExerciseSetResponse(
                            s.Id,
                            s.SetNumber,
                            s.Reps,
                            s.WeightKg,
                            s.DurationSeconds,
                            s.Notes))
                        .ToList()))
                .ToList(),
            new TrainingStatisticsResponse(
                training.Statistics.TotalVolumeKg,
                training.Statistics.TotalSets,
                training.Statistics.TotalReps,
                training.Statistics.EstimatedCalories));
}
