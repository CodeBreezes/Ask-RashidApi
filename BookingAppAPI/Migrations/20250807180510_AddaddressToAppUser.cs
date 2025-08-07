using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingAppAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddaddressToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "AppUsers");
        }
    }
}
