using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    public partial class AddImageRelativeUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RelativeUrl",
                table: "CatalogImage",
                type: "nvarchar(2083)",
                maxLength: 2083,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelativeUrl",
                table: "CatalogImage");
        }
    }
}
