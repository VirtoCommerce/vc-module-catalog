using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    public partial class AddCategoryEditorialReview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableReview",
                table: "Category",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CategoryEditorialReview",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 64, nullable: true),
                    Priority = table.Column<int>(nullable: false),
                    Source = table.Column<string>(maxLength: 128, nullable: true),
                    Content = table.Column<string>(nullable: true),
                    ReviewState = table.Column<int>(nullable: false),
                    Comments = table.Column<string>(nullable: true),
                    Locale = table.Column<string>(maxLength: 64, nullable: true),
                    CategoryId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryEditorialReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryEditorialReview_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryEditorialReview_CategoryId",
                table: "CategoryEditorialReview",
                column: "CategoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryEditorialReview");

            migrationBuilder.DropColumn(
                name: "EnableReview",
                table: "Category");
        }
    }
}
