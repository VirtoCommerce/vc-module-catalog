using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.SqlServer.Migrations
{
    public partial class FixItemCodeIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Code_CatalogId",
                table: "Item");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Category_Parent_category_check",
                table: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Code_CatalogId",
                table: "Item",
                columns: new[] { "Code", "CatalogId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Category_Parent_category_check",
                table: "Category",
                sql: "\"ParentCategoryId\" != \"Id\"");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Code_CatalogId",
                table: "Item");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Category_Parent_category_check",
                table: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Code_CatalogId",
                table: "Item",
                columns: new[] { "Code", "CatalogId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Category_Parent_category_check",
                table: "Category",
                sql: "ParentCategoryId != Id");
        }
    }
}
