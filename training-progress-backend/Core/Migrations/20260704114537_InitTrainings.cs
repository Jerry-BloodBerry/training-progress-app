using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    /// <inheritdoc />
    public partial class InitTrainings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Statistics = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "exercise_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exercise_entries_trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercise_sets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    SetNumber = table.Column<int>(type: "integer", nullable: false),
                    Reps = table.Column<int>(type: "integer", nullable: true),
                    WeightKg = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_sets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exercise_sets_exercise_entries_ExerciseEntryId",
                        column: x => x.ExerciseEntryId,
                        principalTable: "exercise_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exercise_entries_TrainingId",
                table: "exercise_entries",
                column: "TrainingId");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_sets_ExerciseEntryId",
                table: "exercise_sets",
                column: "ExerciseEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_trainings_UserId",
                table: "trainings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_trainings_UserId_Date",
                table: "trainings",
                columns: new[] { "UserId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exercise_sets");

            migrationBuilder.DropTable(
                name: "exercise_entries");

            migrationBuilder.DropTable(
                name: "trainings");
        }
    }
}
