using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    public partial class RemoveOrphanCatalogEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE [CatalogImage]
WHERE ItemId is NULL AND CategoryId is NULL;

DELETE [CatalogSeoInfo]
WHERE ItemId is NULL AND CategoryId is NULL;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not needed
        }
    }
}
