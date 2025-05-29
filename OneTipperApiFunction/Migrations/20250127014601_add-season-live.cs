using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneTipperApiFunction.Migrations
{
    /// <inheritdoc />
    public partial class addseasonlive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Live",
                table: "Seasons",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Live",
                table: "Seasons");
        }
    }
}
