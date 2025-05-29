using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneTipperApiFunction.Migrations
{
    /// <inheritdoc />
    public partial class addmatchfields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "AwayOdds",
                table: "Matches",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "GameTime",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "HomeOdds",
                table: "Matches",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwayOdds",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "GameTime",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "HomeOdds",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Matches");
        }
    }
}
