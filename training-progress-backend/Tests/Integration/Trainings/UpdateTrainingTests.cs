using API.Trainings.CreateTraining;
using API.Trainings.Shared;
using API.Trainings.UpdateTraining;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration.Trainings;

public sealed class UpdateTrainingTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public UpdateTrainingTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Put_ExistingTraining_Returns200WithUpdatedData()
    {
        var created = await CreateTrainingAsync();

        var updateRequest = new UpdateTrainingRequest
        {
            Id = created.Id,
            Date = new DateOnly(2025, 9, 20),
            DurationMinutes = 75,
            Notes = "Updated notes",
            Exercises =
            [
                new UpdateExerciseEntryRequest(
                    ExerciseName: "Deadlift",
                    OrderIndex: 1,
                    Sets:
                    [
                        new UpdateExerciseSetRequest(1, 5, 120m, null, null),
                    ]),
            ],
        };

        var response = await _client.PutAsJsonAsync($"/trainings/{created.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TrainingResponse>();
        Assert.NotNull(body);
        Assert.Equal(new DateOnly(2025, 9, 20), body.Date);
        Assert.Equal(75, body.DurationMinutes);
        Assert.Equal("Updated notes", body.Notes);
        Assert.Single(body.Exercises);
        Assert.Equal("Deadlift", body.Exercises[0].ExerciseName);
        Assert.Equal(5, body.Statistics.TotalReps);
        Assert.Equal(600m, body.Statistics.TotalVolumeKg);
    }

    [Fact]
    public async Task Put_NonExistentId_Returns404()
    {
        var updateRequest = new UpdateTrainingRequest
        {
            Id = Guid.NewGuid(),
            Date = new DateOnly(2025, 1, 1),
            DurationMinutes = 30,
            Notes = null,
            Exercises = [],
        };

        var response = await _client.PutAsJsonAsync($"/trainings/{updateRequest.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<TrainingResponse> CreateTrainingAsync()
    {
        var request = new CreateTrainingRequest(
            Date: new DateOnly(2025, 8, 1),
            DurationMinutes: 50,
            Notes: "Original",
            Exercises: []);

        var response = await _client.PostAsJsonAsync("/trainings", request);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<TrainingResponse>())!;
    }
}
