using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomaticLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAutomatic",
                table: "CategoryItemRelation",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AutomaticLinkQuery",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TargetCategoryId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceCatalogId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SourceCatalogQuery = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomaticLinkQuery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomaticLinkQuery_Catalog_SourceCatalogId",
                        column: x => x.SourceCatalogId,
                        principalTable: "Catalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AutomaticLinkQuery_Category_TargetCategoryId",
                        column: x => x.TargetCategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticLinkQuery_SourceCatalogId",
                table: "AutomaticLinkQuery",
                column: "SourceCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticLinkQuery_TargetCategoryId",
                table: "AutomaticLinkQuery",
                column: "TargetCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomaticLinkQuery");

            migrationBuilder.DropColumn(
                name: "IsAutomatic",
                table: "CategoryItemRelation");
        }
    }
}
