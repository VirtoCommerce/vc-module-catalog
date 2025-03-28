using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasurePropertyAndUnitLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasureId",
                table: "PropertyValue",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeasureId",
                table: "Property",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MeasureUnitLocalizedName",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    ParentEntityId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasureUnitLocalizedName", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeasureUnitLocalizedName_MeasureUnit_ParentEntityId",
                        column: x => x.ParentEntityId,
                        principalTable: "MeasureUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeasureUnitLocalizedSymbol",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    ParentEntityId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasureUnitLocalizedSymbol", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeasureUnitLocalizedSymbol_MeasureUnit_ParentEntityId",
                        column: x => x.ParentEntityId,
                        principalTable: "MeasureUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeasureUnitLocalizedName_LanguageCode_ParentEntityId",
                table: "MeasureUnitLocalizedName",
                columns: new[] { "LanguageCode", "ParentEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeasureUnitLocalizedName_ParentEntityId",
                table: "MeasureUnitLocalizedName",
                column: "ParentEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_MeasureUnitLocalizedSymbol_LanguageCode_ParentEntityId",
                table: "MeasureUnitLocalizedSymbol",
                columns: new[] { "LanguageCode", "ParentEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeasureUnitLocalizedSymbol_ParentEntityId",
                table: "MeasureUnitLocalizedSymbol",
                column: "ParentEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeasureUnitLocalizedName");

            migrationBuilder.DropTable(
                name: "MeasureUnitLocalizedSymbol");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasureId",
                table: "PropertyValue");

            migrationBuilder.DropColumn(
                name: "MeasureId",
                table: "Property");
        }
    }
}
