using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyGroupCatalogId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CatalogId",
                table: "PropertyGroup",
                type: "nvarchar(128)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyGroup_CatalogId",
                table: "PropertyGroup",
                column: "CatalogId");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyGroup_Catalog_CatalogId",
                table: "PropertyGroup",
                column: "CatalogId",
                principalTable: "Catalog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyGroup_Catalog_CatalogId",
                table: "PropertyGroup");

            migrationBuilder.DropIndex(
                name: "IX_PropertyGroup_CatalogId",
                table: "PropertyGroup");

            migrationBuilder.DropColumn(
                name: "CatalogId",
                table: "PropertyGroup");
        }
    }
}
