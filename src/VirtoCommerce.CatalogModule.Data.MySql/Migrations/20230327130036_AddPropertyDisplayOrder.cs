using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.MySql.Migrations
{
    public partial class AddPropertyDisplayOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Category_Parent_category_check",
                table: "Category");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Property",
                type: "int",
                nullable: true);

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

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Property");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Category_Parent_category_check",
                table: "Category",
                sql: "ParentCategoryId != Id");
        }
    }
}
