using API.Trainings.CreateTraining;
using API.Trainings.ListTrainings;
using API.Trainings.Shared;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration.Trainings;

public sealed class ListTrainingsTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ListTrainingsTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Get_ReturnsPagedList()
    {
        await CreateTrainingAsync(new DateOnly(2025, 6, 1));
        await CreateTrainingAsync(new DateOnly(2025, 6, 2));

        var response = await _client.GetAsync("/trainings?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ListTrainingsResponse>();
        Assert.NotNull(body);
        Assert.True(body.TotalCount >= 2);
        Assert.True(body.Items.Count >= 2);
        Assert.Equal(1, body.Page);
        Assert.Equal(20, body.PageSize);
    }

    [Fact]
    public async Task Get_ResultsOrderedByDateDescending()
    {
        await CreateTrainingAsync(new DateOnly(2025, 1, 1));
        await CreateTrainingAsync(new DateOnly(2025, 12, 31));

        var response = await _client.GetAsync("/trainings?page=1&pageSize=100");
        var body = await response.Content.ReadFromJsonAsync<ListTrainingsResponse>();

        Assert.NotNull(body);
        // Items should be newest first
        for (var i = 0; i < body.Items.Count - 1; i++)
        {
            Assert.True(body.Items[i].Date >= body.Items[i + 1].Date);
        }
    }

    private async Task CreateTrainingAsync(DateOnly date)
    {
        var request = new CreateTrainingRequest(date, 30, null, []);
        var response = await _client.PostAsJsonAsync("/trainings", request);
        response.EnsureSuccessStatusCode();
    }
}
