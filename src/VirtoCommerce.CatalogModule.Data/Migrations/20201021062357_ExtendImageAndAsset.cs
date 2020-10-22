using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    public partial class ExtendImageAndAsset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AltText",
                table: "CatalogImage",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CatalogImage",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CatalogAsset",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Group",
                table: "CatalogAsset",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "CatalogAsset",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AltText",
                table: "CatalogImage");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CatalogImage");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CatalogAsset");

            migrationBuilder.DropColumn(
                name: "Group",
                table: "CatalogAsset");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "CatalogAsset");
        }
    }
}
