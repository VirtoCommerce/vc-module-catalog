namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewAssociations : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Association", "AssociationGroupId", "dbo.AssociationGroup");
            DropForeignKey("dbo.AssociationGroup", "ItemId", "dbo.Item");
            DropForeignKey("dbo.ItemRelation", "ChildItemId", "dbo.Item");
            DropForeignKey("dbo.ItemRelation", "ParentItemId", "dbo.Item");
            //DropForeignKey("dbo.Association", "ItemId", "dbo.Item");
            DropIndex("dbo.AssociationGroup", new[] { "ItemId" });
            DropIndex("dbo.Association", new[] { "AssociationGroupId" });
            DropIndex("dbo.ItemRelation", new[] { "ChildItemId" });
            DropIndex("dbo.ItemRelation", new[] { "ParentItemId" });
            AddColumn("dbo.Association", "AssociatedItemId", c => c.String(maxLength: 128));
            AddColumn("dbo.Association", "AssociatedCategoryId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Association", "AssociatedItemId");
            CreateIndex("dbo.Association", "AssociatedCategoryId");
            AddForeignKey("dbo.Association", "AssociatedCategoryId", "dbo.Category", "Id");
            AddForeignKey("dbo.Association", "AssociatedItemId", "dbo.Item", "Id");

            Sql(@"UPDATE dbo.Association SET AssociatedItemId = dbo.Association.ItemId, ItemId = AG.ItemId, AssociationType = AG.Name FROM 
                  dbo.AssociationGroup as AG                 
                  WHERE AG.Id = dbo.Association.AssociationGroupId");

            DropColumn("dbo.Association", "AssociationGroupId");
            DropTable("dbo.AssociationGroup");
            DropTable("dbo.ItemRelation");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ItemRelation",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        RelationTypeId = c.String(maxLength: 64),
                        Quantity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GroupName = c.String(nullable: false, maxLength: 64),
                        Priority = c.Int(nullable: false),
                        ChildItemId = c.String(nullable: false, maxLength: 128),
                        ParentItemId = c.String(maxLength: 128),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(),
                        CreatedBy = c.String(maxLength: 64),
                        ModifiedBy = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AssociationGroup",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 128),
                        Description = c.String(maxLength: 512),
                        Priority = c.Int(nullable: false),
                        ItemId = c.String(nullable: false, maxLength: 128),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(),
                        CreatedBy = c.String(maxLength: 64),
                        ModifiedBy = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Association", "AssociationGroupId", c => c.String(nullable: false, maxLength: 128));
            DropForeignKey("dbo.Association", "AssociatedItemId", "dbo.Item");
            DropForeignKey("dbo.Association", "AssociatedCategoryId", "dbo.Category");
            DropIndex("dbo.Association", new[] { "AssociatedCategoryId" });
            DropIndex("dbo.Association", new[] { "AssociatedItemId" });
            DropColumn("dbo.Association", "AssociatedCategoryId");
            DropColumn("dbo.Association", "AssociatedItemId");
            CreateIndex("dbo.ItemRelation", "ParentItemId");
            CreateIndex("dbo.ItemRelation", "ChildItemId");
            CreateIndex("dbo.Association", "AssociationGroupId");
            CreateIndex("dbo.AssociationGroup", "ItemId");
            AddForeignKey("dbo.Association", "ItemId", "dbo.Item", "Id");
            AddForeignKey("dbo.ItemRelation", "ParentItemId", "dbo.Item", "Id");
            AddForeignKey("dbo.ItemRelation", "ChildItemId", "dbo.Item", "Id");
            AddForeignKey("dbo.AssociationGroup", "ItemId", "dbo.Item", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Association", "AssociationGroupId", "dbo.AssociationGroup", "Id", cascadeDelete: true);
        }
    }
}
