using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddProductConfigurationConditionalSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DependsOnSectionId",
                table: "ProductConfigurationSection",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductConfigurationSection_DependsOnSectionId",
                table: "ProductConfigurationSection",
                column: "DependsOnSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductConfigurationSection_ProductConfigurationSection_DependsOnSectionId",
                table: "ProductConfigurationSection",
                column: "DependsOnSectionId",
                principalTable: "ProductConfigurationSection",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductConfigurationSection_ProductConfigurationSection_DependsOnSectionId",
                table: "ProductConfigurationSection");

            migrationBuilder.DropIndex(
                name: "IX_ProductConfigurationSection_DependsOnSectionId",
                table: "ProductConfigurationSection");

            migrationBuilder.DropColumn(
                name: "DependsOnSectionId",
                table: "ProductConfigurationSection");
        }
    }
}
