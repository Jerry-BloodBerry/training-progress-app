namespace API.Trainings.ListTrainings;

public sealed class ListTrainingsRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
