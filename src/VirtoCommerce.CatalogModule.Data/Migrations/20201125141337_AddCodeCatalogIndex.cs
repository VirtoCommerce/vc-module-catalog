using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    public partial class AddCodeCatalogIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Item_Code",
                table: "Item");

            migrationBuilder.CreateIndex(
                name: "IX_Item_Code_CatalogId",
                table: "Item",
                columns: new[] { "Code", "CatalogId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Item_Code_CatalogId",
                table: "Item");

            migrationBuilder.CreateIndex(
                name: "IX_Item_Code",
                table: "Item",
                column: "Code",
                unique: true);
        }
    }
}
