namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AssociationTags : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Association", "Tags", c => c.String(maxLength: 1024));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Association", "Tags");
        }
    }
}
