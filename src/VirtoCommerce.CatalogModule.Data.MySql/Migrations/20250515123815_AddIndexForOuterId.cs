using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexForOuterId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Item_OuterId",
                table: "Item",
                column: "OuterId");

            migrationBuilder.CreateIndex(
                name: "IX_Category_OuterId",
                table: "Category",
                column: "OuterId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_OuterId",
                table: "Catalog",
                column: "OuterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Item_OuterId",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Category_OuterId",
                table: "Category");

            migrationBuilder.DropIndex(
                name: "IX_Catalog_OuterId",
                table: "Catalog");
        }
    }
}
