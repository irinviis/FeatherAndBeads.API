using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeatherAndBeads.API.Migrations
{
    /// <inheritdoc />
    public partial class ChangedPriceWithoutTaxtoProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Product",
                newName: "ShortDescription");

            migrationBuilder.AddColumn<string>(
                name: "LongDescription",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PriceWithoutTax",
                table: "Product",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LongDescription",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "PriceWithoutTax",
                table: "Product");

            migrationBuilder.RenameColumn(
                name: "ShortDescription",
                table: "Product",
                newName: "Description");
        }
    }
}
