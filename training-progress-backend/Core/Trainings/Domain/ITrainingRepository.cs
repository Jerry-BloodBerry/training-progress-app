namespace Core.Trainings.Domain;

public interface ITrainingRepository
{
    Task<Training?> GetByIdAsync(Guid id, string userId, CancellationToken ct = default);

    Task<(IReadOnlyList<Training> Items, int TotalCount)> GetPagedByUserIdAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task AddAsync(Training training, CancellationToken ct = default);
    void Update(Training training);
    void Remove(Training training);
    Task SaveChangesAsync(CancellationToken ct = default);
}
