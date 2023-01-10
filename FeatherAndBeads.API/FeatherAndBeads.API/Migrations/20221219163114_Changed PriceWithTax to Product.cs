using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeatherAndBeads.API.Migrations
{
    /// <inheritdoc />
    public partial class ChangedPriceWithTaxtoProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceWithoutTax",
                table: "Product",
                newName: "PriceWithTax");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceWithTax",
                table: "Product",
                newName: "PriceWithoutTax");
        }
    }
}
