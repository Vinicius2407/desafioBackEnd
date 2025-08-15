using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Engine.Migrations
{
    /// <inheritdoc />
    public partial class add_colulumn_lose_streak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoseStreakCounter",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoseStreakCounter",
                table: "Users");
        }
    }
}
