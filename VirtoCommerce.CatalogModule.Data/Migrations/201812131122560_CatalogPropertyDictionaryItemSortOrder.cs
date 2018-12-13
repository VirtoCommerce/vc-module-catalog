namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CatalogPropertyDictionaryItemSortOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PropertyDictionaryItem", "SortOrder", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PropertyDictionaryItem", "SortOrder");
        }
    }
}
