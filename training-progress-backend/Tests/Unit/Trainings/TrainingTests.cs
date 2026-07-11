using Core.Trainings.Domain;

namespace Tests.Unit.Trainings;

public sealed class TrainingTests
{
    // --- Training.Create ---

    [Fact]
    public void Create_WithValidArguments_ReturnsTraining()
    {
        var training = Training.Create("user_abc", new DateOnly(2025, 1, 15), 60, "Notes");

        Assert.Equal("user_abc", training.UserId);
        Assert.Equal(new DateOnly(2025, 1, 15), training.Date);
        Assert.Equal(60, training.DurationMinutes);
        Assert.Equal("Notes", training.Notes);
        Assert.NotEqual(Guid.Empty, training.Id);
        Assert.Equal(TrainingStatistics.Empty.TotalVolumeKg, training.Statistics.TotalVolumeKg);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithEmptyUserId_Throws(string userId)
    {
        Assert.Throws<ArgumentException>(() =>
            Training.Create(userId, new DateOnly(2025, 1, 1), 60, null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveDuration_Throws(int duration)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Training.Create("user_abc", new DateOnly(2025, 1, 1), duration, null));
    }

    // --- Training.Update ---

    [Fact]
    public void Update_ChangesDateDurationAndNotes()
    {
        var training = Training.Create("user_abc", new DateOnly(2025, 1, 1), 30, null);

        training.Update(new DateOnly(2025, 6, 1), 90, "Updated");

        Assert.Equal(new DateOnly(2025, 6, 1), training.Date);
        Assert.Equal(90, training.DurationMinutes);
        Assert.Equal("Updated", training.Notes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Update_WithNonPositiveDuration_Throws(int duration)
    {
        var training = Training.Create("user_abc", new DateOnly(2025, 1, 1), 30, null);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            training.Update(new DateOnly(2025, 1, 1), duration, null));
    }

    // --- Training.ReplaceExercises ---

    [Fact]
    public void ReplaceExercises_PopulatesEntries()
    {
        var training = Training.Create("user_abc", new DateOnly(2025, 1, 1), 60, null);
        var exercises = new List<ExerciseEntryData>
        {
            new("Bench Press", 1,
            [
                new ExerciseSetData(1, 10, 80m, null, null),
                new ExerciseSetData(2, 8, 80m, null, null),
            ]),
        };

        training.ReplaceExercises(exercises);

        Assert.Single(training.ExerciseEntries);
        Assert.Equal("Bench Press", training.ExerciseEntries[0].ExerciseName);
        Assert.Equal(2, training.ExerciseEntries[0].Sets.Count);
    }

    [Fact]
    public void ReplaceExercises_ClearsExistingEntries()
    {
        var training = Training.Create("user_abc", new DateOnly(2025, 1, 1), 60, null);
        var first = new List<ExerciseEntryData> { new("Squat", 1, []) };
        var second = new List<ExerciseEntryData> { new("Deadlift", 1, []) };

        training.ReplaceExercises(first);
        training.ReplaceExercises(second);

        Assert.Single(training.ExerciseEntries);
        Assert.Equal("Deadlift", training.ExerciseEntries[0].ExerciseName);
    }

    // --- Training.RecalculateStatistics ---

    [Fact]
    public void RecalculateStatistics_ComputesCorrectTotals()
    {
        var training = Training.Create("user_abc", new DateOnly(2025, 1, 1), 60, null);
        var exercises = new List<ExerciseEntryData>
        {
            new("Bench Press", 1,
            [
                new ExerciseSetData(1, 10, 80m, null, null),  // 800 kg volume
                new ExerciseSetData(2, 8, 80m, null, null),   // 640 kg volume
            ]),
            new("Squat", 2,
            [
                new ExerciseSetData(1, 5, 100m, null, null),  // 500 kg volume
            ]),
        };

        training.ReplaceExercises(exercises);
        training.RecalculateStatistics();

        Assert.Equal(3, training.Statistics.TotalSets);
        Assert.Equal(23, training.Statistics.TotalReps);          // 10 + 8 + 5
        Assert.Equal(1940m, training.Statistics.TotalVolumeKg);   // 800 + 640 + 500
    }

    [Fact]
    public void RecalculateStatistics_WithNoExercises_ReturnsZeroStats()
    {
        var training = Training.Create("user_abc", new DateOnly(2025, 1, 1), 60, null);

        training.RecalculateStatistics();

        Assert.Equal(0, training.Statistics.TotalSets);
        Assert.Equal(0, training.Statistics.TotalReps);
        Assert.Equal(0m, training.Statistics.TotalVolumeKg);
    }

    [Fact]
    public void RecalculateStatistics_BodyweightSets_CountRepsWithZeroVolume()
    {
        var training = Training.Create("user_abc", new DateOnly(2025, 1, 1), 60, null);
        var exercises = new List<ExerciseEntryData>
        {
            new("Pull-up", 1,
            [
                new ExerciseSetData(1, 12, null, null, null),
            ]),
        };

        training.ReplaceExercises(exercises);
        training.RecalculateStatistics();

        Assert.Equal(1, training.Statistics.TotalSets);
        Assert.Equal(12, training.Statistics.TotalReps);
        Assert.Equal(0m, training.Statistics.TotalVolumeKg);
    }
}
