namespace VirtoCommerce.CatalogModule.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddIsHidden : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Property", "IsHidden", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Property", "IsHidden");
        }
    }
}
