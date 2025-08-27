using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoLift.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExistingTripsStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update all existing trips to have Published status (1)
            migrationBuilder.Sql("UPDATE [Trips] SET [Status] = 1 WHERE [Status] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert all trips back to status 0 if needed
            migrationBuilder.Sql("UPDATE [Trips] SET [Status] = 0 WHERE [Status] = 1");
        }
    }
}
