using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class NormDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoolNumber",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "PosleSlovie",
                table: "Notes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CoolNumber",
                table: "Notes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PosleSlovie",
                table: "Notes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
