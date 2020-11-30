using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    public partial class AddCodeCatalogIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF NOT (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__MigrationHistory'))
                    BEGIN
		                DROP INDEX IF EXISTS [IX_Item_Code] ON [Item]
		                CREATE INDEX IX_Item_Code_CatalogId ON [Item](Code, CatalogId);
                    END
                ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF NOT (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__MigrationHistory'))
                    BEGIN
		                DROP INDEX IF EXISTS [IX_Item_Code_CatalogId] ON [Item]
		                CREATE INDEX IX_Item_Code ON [Item](Code);
                    END
                ");
        }
    }
}
