using API.Trainings.Shared;
using Core.Trainings.Domain;
using FastEndpoints;
using System.Security.Claims;

namespace API.Trainings.ListTrainings;

public sealed record ListTrainingsResponse(
    IReadOnlyList<TrainingResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed class ListTrainingsEndpoint : Endpoint<ListTrainingsRequest, ListTrainingsResponse>
{
    private readonly ITrainingRepository _repository;

    public ListTrainingsEndpoint(ITrainingRepository repository) => _repository = repository;

    public override void Configure()
    {
        Get("/trainings");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override async Task HandleAsync(ListTrainingsRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var page = req.Page < 1 ? 1 : req.Page;
        var pageSize = req.PageSize is < 1 or > 100 ? 20 : req.PageSize;

        var (items, totalCount) = await _repository.GetPagedByUserIdAsync(userId, page, pageSize, ct);

        await SendOkAsync(
            new ListTrainingsResponse(
                items.Select(t => t.ToResponse()).ToList(),
                totalCount,
                page,
                pageSize),
            ct);
    }
}
