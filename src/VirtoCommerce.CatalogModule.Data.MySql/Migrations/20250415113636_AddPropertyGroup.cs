using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PropertyGroupId",
                table: "Property",
                type: "varchar(128)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PropertyGroup",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CatalogId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModifiedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyGroup_Catalog_CatalogId",
                        column: x => x.CatalogId,
                        principalTable: "Catalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PropertyGroupLocalizedDescription",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LanguageCode = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentEntityId = table.Column<string>(type: "varchar(128)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyGroupLocalizedDescription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyGroupLocalizedDescription_PropertyGroup_ParentEntity~",
                        column: x => x.ParentEntityId,
                        principalTable: "PropertyGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PropertyGroupLocalizedName",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LanguageCode = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentEntityId = table.Column<string>(type: "varchar(128)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyGroupLocalizedName", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyGroupLocalizedName_PropertyGroup_ParentEntityId",
                        column: x => x.ParentEntityId,
                        principalTable: "PropertyGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Property_PropertyGroupId",
                table: "Property",
                column: "PropertyGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyGroup_CatalogId",
                table: "PropertyGroup",
                column: "CatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyGroupLocalizedDescription_LanguageCode_ParentEntityId",
                table: "PropertyGroupLocalizedDescription",
                columns: new[] { "LanguageCode", "ParentEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyGroupLocalizedDescription_ParentEntityId",
                table: "PropertyGroupLocalizedDescription",
                column: "ParentEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyGroupLocalizedName_LanguageCode_ParentEntityId",
                table: "PropertyGroupLocalizedName",
                columns: new[] { "LanguageCode", "ParentEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyGroupLocalizedName_ParentEntityId",
                table: "PropertyGroupLocalizedName",
                column: "ParentEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Property_PropertyGroup_PropertyGroupId",
                table: "Property",
                column: "PropertyGroupId",
                principalTable: "PropertyGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Property_PropertyGroup_PropertyGroupId",
                table: "Property");

            migrationBuilder.DropTable(
                name: "PropertyGroupLocalizedDescription");

            migrationBuilder.DropTable(
                name: "PropertyGroupLocalizedName");

            migrationBuilder.DropTable(
                name: "PropertyGroup");

            migrationBuilder.DropIndex(
                name: "IX_Property_PropertyGroupId",
                table: "Property");

            migrationBuilder.DropColumn(
                name: "PropertyGroupId",
                table: "Property");
        }
    }
}
