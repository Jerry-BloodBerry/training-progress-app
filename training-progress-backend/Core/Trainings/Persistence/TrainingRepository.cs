using Core.Persistence;
using Core.Trainings.Domain;
using Microsoft.EntityFrameworkCore;

namespace Core.Trainings.Persistence;

internal sealed class TrainingRepository : ITrainingRepository
{
    private readonly AppDbContext _db;

    public TrainingRepository(AppDbContext db) => _db = db;

    public Task<Training?> GetByIdAsync(Guid id, string userId, CancellationToken ct = default) =>
        _db.Trainings
            .Include(t => t.ExerciseEntries)
            .ThenInclude(e => e.Sets)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);

    public async Task<(IReadOnlyList<Training> Items, int TotalCount)> GetPagedByUserIdAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _db.Trainings.Where(t => t.UserId == userId);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(t => t.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(Training training, CancellationToken ct = default) =>
        await _db.Trainings.AddAsync(training, ct);

    public void Update(Training training)
    {
        // training is already tracked; property changes are detected automatically.
        // Explicitly mark new (Detached) ExerciseEntry/ExerciseSet objects as Added
        // so EF Core inserts them instead of issuing UPDATE for non-existent rows.
        foreach (var entry in training.ExerciseEntries)
        {
            if (_db.Entry(entry).State == EntityState.Detached)
            {
                _db.Entry(entry).State = EntityState.Added;
                foreach (var set in entry.Sets)
                    _db.Entry(set).State = EntityState.Added;
            }
        }
    }

    public void Remove(Training training) => _db.Trainings.Remove(training);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
