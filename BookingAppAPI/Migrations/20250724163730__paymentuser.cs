using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingAppAPI.Migrations
{
    /// <inheritdoc />
    public partial class _paymentuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "userId",
                table: "Payments",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "userId",
                table: "Payments");
        }
    }
}
