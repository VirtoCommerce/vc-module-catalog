using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    public partial class AddItemCreatedDateIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS [IX_Item_CreatedDate] ON [Item]
                    IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_Item_CreatedDate' AND object_id = OBJECT_ID('Item'))
                    BEGIN
		                CREATE INDEX [IX_Item_CreatedDate] ON [Item]([CreatedDate] ASC)
                    END
                ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS [IX_Item_CreatedDate] ON [Item]");
        }
    }
}
