using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingAppAPI.Migrations
{
    /// <inheritdoc />
    public partial class _googlelogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GoogleSignIn",
                table: "AppUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleSignIn",
                table: "AppUsers");
        }
    }
}
