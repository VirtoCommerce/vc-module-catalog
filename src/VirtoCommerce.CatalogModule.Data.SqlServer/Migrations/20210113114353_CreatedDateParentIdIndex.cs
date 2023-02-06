using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogModule.Data.SqlServer.Migrations
{
    public partial class CreatedDateParentIdIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CreatedDate_ParentId",
                table: "Item",
                columns: new[] { "CreatedDate", "ParentId" })
                .Annotation("SqlServer:Include", new[] { "ModifiedDate" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CreatedDate_ParentId",
                table: "Item");
        }
    }
}
