using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Catalog_AddCategoryAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ItemId",
                table: "CatalogAsset",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<string>(
                name: "CategoryId",
                table: "CatalogAsset",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogAsset_CategoryId",
                table: "CatalogAsset",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogAsset_Category_CategoryId",
                table: "CatalogAsset",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogAsset_Category_CategoryId",
                table: "CatalogAsset");

            migrationBuilder.DropIndex(
                name: "IX_CatalogAsset_CategoryId",
                table: "CatalogAsset");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "CatalogAsset");

            migrationBuilder.AlterColumn<string>(
                name: "ItemId",
                table: "CatalogAsset",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldNullable: true);
        }
    }
}
