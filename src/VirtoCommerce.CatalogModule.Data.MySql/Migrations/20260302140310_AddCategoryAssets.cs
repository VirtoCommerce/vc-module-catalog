using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ItemId",
                table: "CatalogAsset",
                type: "varchar(128)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(128)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CategoryId",
                table: "CatalogAsset",
                type: "varchar(128)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.UpdateData(
                table: "CatalogAsset",
                keyColumn: "ItemId",
                keyValue: null,
                column: "ItemId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ItemId",
                table: "CatalogAsset",
                type: "varchar(128)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(128)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
