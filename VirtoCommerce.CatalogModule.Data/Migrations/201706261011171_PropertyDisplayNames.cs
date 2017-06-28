namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class PropertyDisplayNames : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PropertyDisplayName",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Locale = c.String(maxLength: 64),
                    Name = c.String(maxLength: 512),
                    PropertyId = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.PropertyId);

            Sql(@"INSERT INTO dbo.PropertyDisplayName ([Id], [Locale], [Name], [PropertyId])
                  SELECT [Id], RIGHT(PropertyAttributeName, LEN(PropertyAttributeName) - LEN('DisplayName')) as [Locale], PropertyAttributeValue as [Name], [PropertyId] FROM dbo.PropertyAttribute
                  WHERE [PropertyAttributeName] LIKE 'DisplayName%'");

        }

        public override void Down()
        {
            DropIndex("dbo.PropertyDisplayName", new[] { "PropertyId" });
            DropTable("dbo.PropertyDisplayName");
        }
    }
}
