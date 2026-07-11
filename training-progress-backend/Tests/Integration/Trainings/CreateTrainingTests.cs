using API.Trainings.CreateTraining;
using API.Trainings.ListTrainings;
using API.Trainings.Shared;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration.Trainings;

public sealed class CreateTrainingTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public CreateTrainingTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Post_WithValidRequest_Returns201WithTraining()
    {
        var request = new CreateTrainingRequest(
            Date: new DateOnly(2025, 3, 10),
            DurationMinutes: 60,
            Notes: "Morning session",
            Exercises:
            [
                new CreateExerciseEntryRequest(
                    ExerciseName: "Bench Press",
                    OrderIndex: 1,
                    Sets:
                    [
                        new CreateExerciseSetRequest(1, 10, 80m, null, null),
                        new CreateExerciseSetRequest(2, 8, 85m, null, null),
                    ]),
            ]);

        var response = await _client.PostAsJsonAsync("/trainings", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TrainingResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal(new DateOnly(2025, 3, 10), body.Date);
        Assert.Equal(60, body.DurationMinutes);
        Assert.Equal("Morning session", body.Notes);
        Assert.Single(body.Exercises);
        Assert.Equal("Bench Press", body.Exercises[0].ExerciseName);
        Assert.Equal(2, body.Exercises[0].Sets.Count);

        // Statistics should be computed by the event handler
        Assert.Equal(2, body.Statistics.TotalSets);
        Assert.Equal(18, body.Statistics.TotalReps);        // 10 + 8
        Assert.Equal(1480m, body.Statistics.TotalVolumeKg); // (80*10) + (85*8) = 800 + 680
    }

    [Fact]
    public async Task Post_WithZeroDuration_Returns400()
    {
        var request = new CreateTrainingRequest(
            Date: new DateOnly(2025, 3, 10),
            DurationMinutes: 0,
            Notes: null,
            Exercises: []);

        var response = await _client.PostAsJsonAsync("/trainings", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithNoExercises_Returns201WithEmptyStats()
    {
        var request = new CreateTrainingRequest(
            Date: new DateOnly(2025, 4, 1),
            DurationMinutes: 30,
            Notes: null,
            Exercises: []);

        var response = await _client.PostAsJsonAsync("/trainings", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TrainingResponse>();
        Assert.NotNull(body);
        Assert.Empty(body.Exercises);
        Assert.Equal(0, body.Statistics.TotalSets);
        Assert.Equal(0m, body.Statistics.TotalVolumeKg);
    }
}
