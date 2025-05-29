using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneTipperApiFunction.Migrations
{
    /// <inheritdoc />
    public partial class addhometipsawaytips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AwayTips",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HomeTips",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwayTips",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "HomeTips",
                table: "Players");
        }
    }
}
