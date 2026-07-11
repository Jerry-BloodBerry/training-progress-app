using API.Trainings.CreateTraining;
using System.Net;
using System.Net.Http.Json;
using Tests.Helpers;

namespace Tests.Integration.Trainings;

/// <summary>
/// Verifies that every training endpoint returns 401 when called without authentication.
/// </summary>
public sealed class UnauthorizedAccessTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public UnauthorizedAccessTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add(FakeAuthHandler.SkipAuthHeader, "true");
    }

    [Fact]
    public async Task Post_Trainings_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/trainings", new CreateTrainingRequest(
            Date: new DateOnly(2025, 1, 1),
            DurationMinutes: 30,
            Notes: null,
            Exercises: []));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_Trainings_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/trainings");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_TrainingById_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync($"/trainings/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Put_Training_WithoutAuth_Returns401()
    {
        var response = await _client.PutAsJsonAsync($"/trainings/{Guid.NewGuid()}", new { });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Training_WithoutAuth_Returns401()
    {
        var response = await _client.DeleteAsync($"/trainings/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
