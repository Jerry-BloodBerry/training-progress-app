using API.Trainings.CreateTraining;
using API.Trainings.Shared;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration.Trainings;

public sealed class GetTrainingTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public GetTrainingTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Get_ExistingTraining_Returns200WithCorrectData()
    {
        var created = await CreateTrainingAsync();

        var response = await _client.GetAsync($"/trainings/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TrainingResponse>();
        Assert.NotNull(body);
        Assert.Equal(created.Id, body.Id);
        Assert.Equal(created.Date, body.Date);
        Assert.Equal(created.DurationMinutes, body.DurationMinutes);
    }

    [Fact]
    public async Task Get_NonExistentId_Returns404()
    {
        var response = await _client.GetAsync($"/trainings/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<TrainingResponse> CreateTrainingAsync()
    {
        var request = new CreateTrainingRequest(
            Date: new DateOnly(2025, 5, 1),
            DurationMinutes: 45,
            Notes: null,
            Exercises: []);

        var response = await _client.PostAsJsonAsync("/trainings", request);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<TrainingResponse>())!;
    }
}
