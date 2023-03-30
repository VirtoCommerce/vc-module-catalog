using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.MySql.Migrations
{
    public partial class UpdateParentCategoryConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Category_Parent_category_check",
                table: "Category");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Category_Parent_category_check",
                table: "Category",
                sql: "\"ParentCategoryId\" != \"Id\"");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Category_Parent_category_check",
                table: "Category");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Category_Parent_category_check",
                table: "Category",
                sql: "ParentCategoryId != Id");
        }
    }
}
