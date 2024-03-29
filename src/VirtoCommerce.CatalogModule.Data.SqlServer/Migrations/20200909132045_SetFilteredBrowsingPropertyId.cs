using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogModule.Data.SqlServer.Migrations
{
    public partial class SetFilteredBrowsingPropertyId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PlatformDynamicProperty'))
                BEGIN
                    DELETE FROM PlatformDynamicPropertyName WHERE PropertyId IN (select Id from [PlatformDynamicProperty]
                        WHERE Name = 'FilteredBrowsing' AND ObjectType = 'VirtoCommerce.StoreModule.Core.Model.Store')
                    DELETE FROM PlatformDynamicPropertyObjectValue WHERE PropertyId IN (select Id from [PlatformDynamicProperty]
                        WHERE Name = 'FilteredBrowsing' AND ObjectType = 'VirtoCommerce.StoreModule.Core.Model.Store')
                    UPDATE [PlatformDynamicProperty] SET Id = 'VirtoCommerce.Catalog_FilteredBrowsing_Property'
                        WHERE Name = 'FilteredBrowsing' AND ObjectType = 'VirtoCommerce.StoreModule.Core.Model.Store' AND Id != 'VirtoCommerce.Catalog_FilteredBrowsing_Property'
                END");

            migrationBuilder.Sql(@"IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'StoreDynamicPropertyObjectValue'))
                BEGIN
                    UPDATE [StoreDynamicPropertyObjectValue] SET PropertyId = 'VirtoCommerce.Catalog_FilteredBrowsing_Property'
                        WHERE PropertyName = 'FilteredBrowsing' AND ObjectType = 'VirtoCommerce.StoreModule.Core.Model.Store'	
                END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Method intentionally left empty.
        }
    }
}
