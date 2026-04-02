using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class TestUpdate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PosleSlovie",
                table: "Notes",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PosleSlovie",
                table: "Notes");
        }
    }
}
