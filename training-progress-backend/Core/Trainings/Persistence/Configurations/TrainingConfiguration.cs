using Core.Trainings.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Trainings.Persistence.Configurations;

internal sealed class TrainingConfiguration : IEntityTypeConfiguration<Training>
{
    public void Configure(EntityTypeBuilder<Training> builder)
    {
        builder.ToTable("trainings");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId).IsRequired().HasMaxLength(128);
        builder.Property(t => t.Notes).HasMaxLength(1000);

        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => new { t.UserId, t.Date });

        builder.OwnsOne(t => t.Statistics, b => b.ToJson());

        builder
            .HasMany(t => t.ExerciseEntries)
            .WithOne()
            .HasForeignKey(e => e.TrainingId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(t => t.ExerciseEntries)
            .HasField("_exerciseEntries")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
