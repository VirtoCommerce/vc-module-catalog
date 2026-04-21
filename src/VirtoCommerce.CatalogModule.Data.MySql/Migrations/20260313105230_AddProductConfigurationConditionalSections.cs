using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.MySql.Migrations
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
                type: "varchar(128)",
                maxLength: 128,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProductConfigurationSection_DependsOnSectionId",
                table: "ProductConfigurationSection",
                column: "DependsOnSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductConfigurationSection_ProductConfigurationSection_Depe~",
                table: "ProductConfigurationSection",
                column: "DependsOnSectionId",
                principalTable: "ProductConfigurationSection",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductConfigurationSection_ProductConfigurationSection_Depe~",
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
