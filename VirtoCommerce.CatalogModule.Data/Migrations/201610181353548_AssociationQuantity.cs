namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AssociationQuantity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Association", "Quantity", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Association", "Quantity");
        }
    }
}
