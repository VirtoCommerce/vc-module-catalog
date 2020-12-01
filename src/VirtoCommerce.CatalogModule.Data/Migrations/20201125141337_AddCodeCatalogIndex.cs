using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    public partial class AddCodeCatalogIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS [IX_Item_Code] ON [Item]
                    IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_Code_CatalogId' AND object_id = OBJECT_ID('Item'))
                    BEGIN
		                CREATE INDEX IX_Code_CatalogId ON [Item](Code, CatalogId);
                    END
                ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS [IX_Code_CatalogId] ON [Item]
                    IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_Item_Code' AND object_id = OBJECT_ID('Item'))
                    BEGIN
		                CREATE INDEX IX_Item_Code ON [Item](Code);
                    END
                ");
        }
    }
}
