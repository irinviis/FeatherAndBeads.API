using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeatherAndBeads.API.Migrations
{
    /// <inheritdoc />
    public partial class addedpublicIdtoPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "Photo",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Photo");
        }
    }
}
