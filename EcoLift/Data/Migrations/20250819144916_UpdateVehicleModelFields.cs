using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoLift.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVehicleModelFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Year",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "RegistrationNumber",
                table: "Vehicles",
                newName: "LicensePlate");

            migrationBuilder.RenameColumn(
                name: "Make",
                table: "Vehicles",
                newName: "Brand");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_RegistrationNumber",
                table: "Vehicles",
                newName: "IX_Vehicles_LicensePlate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LicensePlate",
                table: "Vehicles",
                newName: "RegistrationNumber");

            migrationBuilder.RenameColumn(
                name: "Brand",
                table: "Vehicles",
                newName: "Make");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles",
                newName: "IX_Vehicles_RegistrationNumber");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
