using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class PropertyGroupConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Property_PropertyGroup_PropertyGroupId",
                table: "Property");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyGroup_Catalog_CatalogId",
                table: "PropertyGroup");

            migrationBuilder.AddForeignKey(
                name: "FK_Property_PropertyGroup_PropertyGroupId",
                table: "Property",
                column: "PropertyGroupId",
                principalTable: "PropertyGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyGroup_Catalog_CatalogId",
                table: "PropertyGroup",
                column: "CatalogId",
                principalTable: "Catalog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Property_PropertyGroup_PropertyGroupId",
                table: "Property");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyGroup_Catalog_CatalogId",
                table: "PropertyGroup");

            migrationBuilder.AddForeignKey(
                name: "FK_Property_PropertyGroup_PropertyGroupId",
                table: "Property",
                column: "PropertyGroupId",
                principalTable: "PropertyGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyGroup_Catalog_CatalogId",
                table: "PropertyGroup",
                column: "CatalogId",
                principalTable: "Catalog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
