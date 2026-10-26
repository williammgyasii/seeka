using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seeka.ResourceAccess.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompensationRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MaximumCompensation",
                table: "JobPreferences",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaximumCompensation",
                table: "JobPreferences");
        }
    }
}
