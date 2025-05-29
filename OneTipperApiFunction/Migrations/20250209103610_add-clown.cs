using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneTipperApiFunction.Migrations
{
    /// <inheritdoc />
    public partial class addclown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Clown",
                table: "Tips",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Clown",
                table: "Tips");
        }
    }
}
