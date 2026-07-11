using Core.Trainings.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Trainings.Persistence.Configurations;

internal sealed class ExerciseSetConfiguration : IEntityTypeConfiguration<ExerciseSet>
{
    public void Configure(EntityTypeBuilder<ExerciseSet> builder)
    {
        builder.ToTable("exercise_sets");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.WeightKg).HasPrecision(8, 2);
        builder.Property(s => s.Notes).HasMaxLength(500);
    }
}
