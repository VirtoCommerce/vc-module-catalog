using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogSeoInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CatalogId",
                table: "CatalogSeoInfo",
                type: "varchar(128)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogSeoInfo_CatalogId",
                table: "CatalogSeoInfo",
                column: "CatalogId");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogSeoInfo_Catalog_CatalogId",
                table: "CatalogSeoInfo",
                column: "CatalogId",
                principalTable: "Catalog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogSeoInfo_Catalog_CatalogId",
                table: "CatalogSeoInfo");

            migrationBuilder.DropIndex(
                name: "IX_CatalogSeoInfo_CatalogId",
                table: "CatalogSeoInfo");

            migrationBuilder.DropColumn(
                name: "CatalogId",
                table: "CatalogSeoInfo");
        }
    }
}
