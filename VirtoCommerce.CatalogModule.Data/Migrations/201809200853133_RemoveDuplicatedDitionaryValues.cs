namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RemoveDuplicatedDitionaryValues : DbMigration
    {
        public override void Up()
        {
            Sql(@"
             WITH cte AS(
              SELECT *,
                 row_number() OVER(PARTITION BY Value, Locale, DictionaryItemId ORDER BY DictionaryItemId) AS[rn]
            
              FROM [dbo].[PropertyDictionaryValue]
            
            )
            Delete from cte WHERE[rn] > 1");
        }

        public override void Down()
        {
        }
    }
}
