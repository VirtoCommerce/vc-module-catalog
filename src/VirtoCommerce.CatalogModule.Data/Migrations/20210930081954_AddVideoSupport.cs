using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    public partial class AddVideoSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogVideo",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 64, nullable: true),
                    Name = table.Column<string>(maxLength: 1024, nullable: false),
                    Description = table.Column<string>(maxLength: 1024, nullable: false),
                    SortOrder = table.Column<int>(nullable: false),
                    UploadDate = table.Column<DateTime>(nullable: false),
                    ThumbnailUrl = table.Column<string>(maxLength: 2083, nullable: false),
                    ContentUrl = table.Column<string>(maxLength: 2083, nullable: false),
                    EmbedUrl = table.Column<string>(maxLength: 2083, nullable: true),
                    Duration = table.Column<string>(maxLength: 20, nullable: true),
                    LanguageCode = table.Column<string>(maxLength: 5, nullable: false),
                    OwnerId = table.Column<string>(maxLength: 128, nullable: false),
                    OwnerType = table.Column<string>(maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogVideo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OwnerType_OwnerId",
                table: "CatalogVideo",
                columns: new[] { "OwnerType", "OwnerId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogVideo");
        }
    }
}
