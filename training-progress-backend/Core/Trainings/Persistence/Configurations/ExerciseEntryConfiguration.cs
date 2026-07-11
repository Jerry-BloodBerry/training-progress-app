using Core.Trainings.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Trainings.Persistence.Configurations;

internal sealed class ExerciseEntryConfiguration : IEntityTypeConfiguration<ExerciseEntry>
{
    public void Configure(EntityTypeBuilder<ExerciseEntry> builder)
    {
        builder.ToTable("exercise_entries");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ExerciseName).IsRequired().HasMaxLength(200);

        builder
            .HasMany(e => e.Sets)
            .WithOne()
            .HasForeignKey(s => s.ExerciseEntryId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(e => e.Sets)
            .HasField("_sets")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
