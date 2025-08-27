using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoLift.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTripModelFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trip_DepartureCity_DepartureTime",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trip_DestinationCity_DepartureTime",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "DepartureLatitude",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "DepartureLongitude",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "DestinationLatitude",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "DestinationLongitude",
                table: "Trips");

            migrationBuilder.RenameColumn(
                name: "DestinationCity",
                table: "Trips",
                newName: "PickupLocation");

            migrationBuilder.RenameColumn(
                name: "DepartureCity",
                table: "Trips",
                newName: "DropoffLocation");

            migrationBuilder.RenameColumn(
                name: "ArrivalTime",
                table: "Trips",
                newName: "DepartureDate");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "DepartureTime",
                table: "Trips",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<bool>(
                name: "AllowPets",
                table: "Trips",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowSmoking",
                table: "Trips",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "DropoffLatitude",
                table: "Trips",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DropoffLongitude",
                table: "Trips",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Trips",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PickupLatitude",
                table: "Trips",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PickupLongitude",
                table: "Trips",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trip_DropoffLocation_DepartureDateTime",
                table: "Trips",
                columns: new[] { "DropoffLocation", "DepartureDate", "DepartureTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Trip_PickupLocation_DepartureDateTime",
                table: "Trips",
                columns: new[] { "PickupLocation", "DepartureDate", "DepartureTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trip_DropoffLocation_DepartureDateTime",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trip_PickupLocation_DepartureDateTime",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "AllowPets",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "AllowSmoking",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "DropoffLatitude",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "DropoffLongitude",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "PickupLatitude",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "PickupLongitude",
                table: "Trips");

            migrationBuilder.RenameColumn(
                name: "PickupLocation",
                table: "Trips",
                newName: "DestinationCity");

            migrationBuilder.RenameColumn(
                name: "DropoffLocation",
                table: "Trips",
                newName: "DepartureCity");

            migrationBuilder.RenameColumn(
                name: "DepartureDate",
                table: "Trips",
                newName: "ArrivalTime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DepartureTime",
                table: "Trips",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AddColumn<decimal>(
                name: "DepartureLatitude",
                table: "Trips",
                type: "decimal(9,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DepartureLongitude",
                table: "Trips",
                type: "decimal(9,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DestinationLatitude",
                table: "Trips",
                type: "decimal(9,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DestinationLongitude",
                table: "Trips",
                type: "decimal(9,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Trip_DepartureCity_DepartureTime",
                table: "Trips",
                columns: new[] { "DepartureCity", "DepartureTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Trip_DestinationCity_DepartureTime",
                table: "Trips",
                columns: new[] { "DestinationCity", "DepartureTime" });
        }
    }
}
