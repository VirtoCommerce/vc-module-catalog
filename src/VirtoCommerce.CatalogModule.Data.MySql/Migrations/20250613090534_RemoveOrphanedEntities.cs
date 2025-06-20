using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogModule.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOrphanedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM `Association` WHERE `ItemId` IS NULL AND `AssociatedItemId` IS NULL AND `AssociatedCategoryId` IS NULL;
                DELETE FROM `CategoryItemRelation` WHERE `ItemId` IS NULL AND `CategoryId` IS NULL AND `CatalogId` IS NULL;
                DELETE FROM `CategoryRelation` WHERE `SourceCategoryId` IS NULL AND `TargetCatalogId` IS NULL AND `TargetCategoryId` IS NULL;
                DELETE FROM `CatalogImage` WHERE `ItemId` IS NULL AND `CategoryId` IS NULL;
                DELETE FROM `Property` WHERE `CatalogId` IS NULL AND `CategoryId` IS NULL;
                DELETE FROM `ProductConfigurationOption` WHERE `SectionId` IS NULL AND `ProductId` IS NULL;
                DELETE FROM `PropertyValue` WHERE `ItemId` IS NULL AND `CategoryId` IS NULL AND `CatalogId` IS NULL AND `DictionaryItemId` IS NULL;
                DELETE FROM `CatalogSeoInfo` WHERE `ItemId` IS NULL AND `CategoryId` IS NULL AND `CatalogId` IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
