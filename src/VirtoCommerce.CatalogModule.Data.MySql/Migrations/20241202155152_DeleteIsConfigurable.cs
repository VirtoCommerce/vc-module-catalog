using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class DeleteIsConfigurable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsConfigurable",
                table: "Item");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsConfigurable",
                table: "Item",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
