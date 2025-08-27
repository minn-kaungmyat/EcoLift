using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoLift.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationIdToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConversationId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ConversationId",
                table: "Bookings",
                column: "ConversationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Conversations_ConversationId",
                table: "Bookings",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "ConversationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Conversations_ConversationId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ConversationId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ConversationId",
                table: "Bookings");
        }
    }
}
