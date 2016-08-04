namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ItemPriority : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Item", "Priority", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Item", "Priority");
        }
    }
}
