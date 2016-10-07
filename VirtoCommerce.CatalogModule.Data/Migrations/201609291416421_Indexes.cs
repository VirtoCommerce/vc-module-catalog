namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Indexes : DbMigration
    {
        public override void Up()
        {
            Sql("CREATE NONCLUSTERED INDEX IX_ParentId ON dbo.Item (ParentId) INCLUDE (Id)");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Item", "IX_ParentId");
        }
    }
}
