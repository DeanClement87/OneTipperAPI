using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneTipperApiFunction.Migrations
{
    /// <inheritdoc />
    public partial class addshowtips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowTips",
                table: "Rounds",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowTips",
                table: "Rounds");
        }
    }
}
