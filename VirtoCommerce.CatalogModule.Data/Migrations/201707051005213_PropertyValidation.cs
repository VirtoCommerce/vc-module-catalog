namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PropertyValidation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PropertyValidationRule",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        IsUnique = c.Boolean(nullable: false),
                        CharCountMin = c.Int(),
                        CharCountMax = c.Int(),
                        RegExp = c.String(maxLength: 2048),
                        PropertyId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Property", t => t.PropertyId, cascadeDelete: true)
                .Index(t => t.PropertyId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PropertyValidationRule", "PropertyId", "dbo.Property");
            DropIndex("dbo.PropertyValidationRule", new[] { "PropertyId" });
            DropTable("dbo.PropertyValidationRule");
        }
    }
}
