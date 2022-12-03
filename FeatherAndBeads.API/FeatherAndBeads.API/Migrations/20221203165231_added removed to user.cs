using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeatherAndBeads.API.Migrations
{
    /// <inheritdoc />
    public partial class addedremovedtouser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Removed",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Removed",
                table: "User");
        }
    }
}
