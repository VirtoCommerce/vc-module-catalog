namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PropertyValueCascadeDelete : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PropertyValue", "DictionaryItemId", "dbo.PropertyDictionaryItem");
            AddForeignKey("dbo.PropertyValue", "DictionaryItemId", "dbo.PropertyDictionaryItem", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PropertyValue", "DictionaryItemId", "dbo.PropertyDictionaryItem");
            AddForeignKey("dbo.PropertyValue", "DictionaryItemId", "dbo.PropertyDictionaryItem", "Id");
        }
    }
}
