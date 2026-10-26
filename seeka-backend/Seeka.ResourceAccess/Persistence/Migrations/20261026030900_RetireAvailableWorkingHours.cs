using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seeka.ResourceAccess.Persistence.Migrations
{
    /// <summary>
    /// The model no longer maps AvailableWorkingHours. The column stays, with a default, until no deployed
    /// release reads it; a later migration drops it.
    /// </summary>
    public partial class RetireAvailableWorkingHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER TABLE "JobPreferences" ALTER COLUMN "AvailableWorkingHours" SET DEFAULT '';""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER TABLE "JobPreferences" ALTER COLUMN "AvailableWorkingHours" DROP DEFAULT;""");
        }
    }
}
