using API.Trainings.Shared;
using Core.Trainings.Domain;
using FastEndpoints;
using System.Security.Claims;

namespace API.Trainings.GetTraining;

public sealed class GetTrainingEndpoint : Endpoint<GetTrainingRequest, TrainingResponse>
{
    private readonly ITrainingRepository _repository;

    public GetTrainingEndpoint(ITrainingRepository repository) => _repository = repository;

    public override void Configure()
    {
        Get("/trainings/{id}");
        // TODO: Remove AllowAnonymous and configure Clerk JWT bearer authentication
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetTrainingRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var training = await _repository.GetByIdAsync(req.Id, userId, ct);
        if (training is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(training.ToResponse(), ct);
    }
}
