using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seeka.ResourceAccess.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFullCandidateProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "CompletedSteps",
                table: "CandidateProfiles",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "DetailsJson",
                table: "CandidateProfiles",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Headline",
                table: "CandidateProfiles",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OnboardingCompletedAt",
                table: "CandidateProfiles",
                type: "timestamp with time zone",
                nullable: true);

            // Profiles saved by the one-form onboarding already finished it.
            migrationBuilder.Sql("""
                UPDATE "CandidateProfiles"
                SET "OnboardingCompletedAt" = "UpdatedAt",
                    "CompletedSteps" = ARRAY['about', 'preferences']
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedSteps",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "DetailsJson",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "Headline",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "OnboardingCompletedAt",
                table: "CandidateProfiles");
        }
    }
}
