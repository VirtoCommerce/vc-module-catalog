using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddProductConfigurationOptionText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowCustomText",
                table: "ProductConfigurationSection",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowPredefinedOptions",
                table: "ProductConfigurationSection",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "ProductId",
                table: "ProductConfigurationOption",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "ProductConfigurationOption",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowCustomText",
                table: "ProductConfigurationSection");

            migrationBuilder.DropColumn(
                name: "AllowPredefinedOptions",
                table: "ProductConfigurationSection");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "ProductConfigurationOption");

            migrationBuilder.AlterColumn<string>(
                name: "ProductId",
                table: "ProductConfigurationOption",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);
        }
    }
}
