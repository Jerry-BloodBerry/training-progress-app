using API.Trainings.CreateTraining;
using API.Trainings.Shared;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration.Trainings;

public sealed class DeleteTrainingTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public DeleteTrainingTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Delete_ExistingTraining_Returns204()
    {
        var created = await CreateTrainingAsync();

        var response = await _client.DeleteAsync($"/trainings/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingTraining_MakesItUnreachable()
    {
        var created = await CreateTrainingAsync();

        await _client.DeleteAsync($"/trainings/{created.Id}");
        var getResponse = await _client.GetAsync($"/trainings/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistentId_Returns404()
    {
        var response = await _client.DeleteAsync($"/trainings/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<TrainingResponse> CreateTrainingAsync()
    {
        var request = new CreateTrainingRequest(
            Date: new DateOnly(2025, 11, 5),
            DurationMinutes: 40,
            Notes: null,
            Exercises: []);

        var response = await _client.PostAsJsonAsync("/trainings", request);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<TrainingResponse>())!;
    }
}
