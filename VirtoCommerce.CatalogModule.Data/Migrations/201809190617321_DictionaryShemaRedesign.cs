namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class DictionaryShemaRedesign : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PropertyDictionaryValue", "PropertyId", "dbo.Property");
            DropIndex("dbo.PropertyDictionaryValue", new[] { "PropertyId" });
            CreateTable(
               "dbo.PropertyDictionaryItem",
               c => new
               {
                   Id = c.String(nullable: false, maxLength: 128),
                   Alias = c.String(nullable: false, maxLength: 512),
                   PropertyId = c.String(nullable: false, maxLength: 128),
               })
               .PrimaryKey(t => t.Id)
               .ForeignKey("dbo.Property", t => t.PropertyId, cascadeDelete: true)
               .Index(t => new { t.Alias, t.PropertyId }, unique: true, name: "IX_AliasAndPropertyId");

            AddColumn("dbo.PropertyValue", "DictionaryItemId", c => c.String(maxLength: 128));
            AddColumn("dbo.PropertyDictionaryValue", "DictionaryItemId", c => c.String(nullable: false, maxLength: 128));

            Sql(@"
                INSERT INTO [dbo].PropertyDictionaryItem (Id, Alias, PropertyId) SELECT cast(LOWER(REPLACE( NEWID(), '-', '')) as nvarchar(128)) as Id, PDV.Alias, PDV.PropertyId  FROM PropertyDictionaryValue AS PDV
                GROUP BY  PDV.PropertyId, PDV.Alias");

            Sql(@"
                UPDATE
                    PV
                SET
                    PV.DictionaryItemId = PDI.Id
                FROM [dbo].PropertyValue  PV
                JOIN [dbo].PropertyDictionaryValue AS PDV ON PV.KeyValue = PDV.Id  
                JOIN [dbo].PropertyDictionaryItem AS PDI ON PDI.PropertyId =  PDV.PropertyId AND PDI.Alias = PDV.Alias");

            Sql(@"UPDATE
                    PDV
                SET
                    PDV.DictionaryItemId = PDI.Id
                FROM [dbo].PropertyDictionaryValue  PDV
                JOIN [dbo].PropertyDictionaryItem AS PDI ON PDI.PropertyId =  PDV.PropertyId AND PDI.Alias = PDV.Alias");

            Sql(@"
            WITH cte AS(
              SELECT *,
                 row_number() OVER(PARTITION BY DictionaryItemId, ItemId, CatalogId ORDER BY DictionaryItemId) AS[rn]
            
              FROM [dbo].PropertyValue
            
              WHERE DictionaryItemId IS NOT NULL
            )
            Delete from cte WHERE[rn] > 1");

            CreateIndex("dbo.PropertyValue", "DictionaryItemId");
            CreateIndex("dbo.PropertyDictionaryValue", "DictionaryItemId");
            AddForeignKey("dbo.PropertyDictionaryValue", "DictionaryItemId", "dbo.PropertyDictionaryItem", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PropertyValue", "DictionaryItemId", "dbo.PropertyDictionaryItem", "Id");

            DropColumn("dbo.PropertyValue", "Alias");
            DropColumn("dbo.PropertyValue", "KeyValue");
            DropColumn("dbo.PropertyDictionaryValue", "Alias");
            DropColumn("dbo.PropertyDictionaryValue", "Name");
            DropColumn("dbo.PropertyDictionaryValue", "PropertyId");
        }

        public override void Down()
        {
            //Nothing 
        }
    }
}
