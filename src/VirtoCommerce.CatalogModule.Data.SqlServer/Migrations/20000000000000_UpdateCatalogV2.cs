using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogModule.Data.SqlServer.Migrations
{
    public partial class UpdateCatalogV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__MigrationHistory'))
                IF (EXISTS (SELECT * FROM __MigrationHistory WHERE ContextKey = 'VirtoCommerce.CatalogModule.Data.Migrations.Configuration'))
                    BEGIN
                        BEGIN
	                        INSERT INTO [__EFMigrationsHistory] ([MigrationId],[ProductVersion]) VALUES ('20190515064457_InitialCatalog', '2.2.3-servicing-35854')
                        END
                    END");

            migrationBuilder.Sql(@"IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__MigrationHistory'))
                IF (EXISTS (SELECT * FROM __MigrationHistory WHERE ContextKey = 'VirtoCommerce.CatalogModule.Data.Migrations.Configuration'))
                    BEGIN
                        CREATE TABLE [CatalogSeoInfo](
	                        [Id] [nvarchar](128) NOT NULL,
	                        [CreatedDate] [datetime2](7) NOT NULL,
	                        [ModifiedDate] [datetime2](7) NULL,
	                        [CreatedBy] [nvarchar](64) NULL,
	                        [ModifiedBy] [nvarchar](64) NULL,
	                        [Keyword] [nvarchar](255) NOT NULL,
	                        [StoreId] [nvarchar](128) NULL,
	                        [IsActive] [bit] NOT NULL,
	                        [Language] [nvarchar](5) NULL,
	                        [Title] [nvarchar](255) NULL,
	                        [MetaDescription] [nvarchar](1024) NULL,
	                        [MetaKeywords] [nvarchar](255) NULL,
	                        [ImageAltDescription] [nvarchar](255) NULL,
	                        [ItemId] [nvarchar](128) NULL,
	                        [CategoryId] [nvarchar](128) NULL,
                         CONSTRAINT [PK_CatalogSeoInfo] PRIMARY KEY CLUSTERED 
                        (
	                        [Id] ASC
                        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                        ) ON [PRIMARY]

                        BEGIN
                            ALTER TABLE [CatalogSeoInfo]  WITH CHECK ADD  CONSTRAINT [FK_CatalogSeoInfo_Category_CategoryId] FOREIGN KEY([CategoryId])
                            REFERENCES [Category] ([Id])

                            ALTER TABLE [CatalogSeoInfo] CHECK CONSTRAINT [FK_CatalogSeoInfo_Category_CategoryId]

                            ALTER TABLE [CatalogSeoInfo]  WITH CHECK ADD  CONSTRAINT [FK_CatalogSeoInfo_Item_ItemId] FOREIGN KEY([ItemId])
                            REFERENCES [Item] ([Id])

                            ALTER TABLE [CatalogSeoInfo] CHECK CONSTRAINT [FK_CatalogSeoInfo_Item_ItemId]
                        END
	                    
				    END");

            migrationBuilder.Sql(@"IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__MigrationHistory'))
                IF (EXISTS (SELECT * FROM __MigrationHistory WHERE ContextKey = 'VirtoCommerce.CatalogModule.Data.Migrations.Configuration'))
                    BEGIN
                        INSERT INTO [CatalogSeoInfo]([Id], [CreatedDate], [ModifiedDate], [CreatedBy], [ModifiedBy], [Keyword], [StoreId], [IsActive], [Language], [Title], [MetaDescription], [MetaKeywords], [ImageAltDescription], [CategoryId])
                            SELECT seo.[Id], seo.[CreatedDate], seo.[ModifiedDate], seo.[CreatedBy], seo.[ModifiedBy], seo.[Keyword], seo.[StoreId], seo.[IsActive], seo.[Language], seo.[Title], seo.[MetaDescription], seo.[MetaKeywords], seo.[ImageAltDescription], seo.[ObjectId] as CategoryId 
                            FROM [SeoUrlKeyword] seo
                            INNER JOIN Category ON Category.Id = seo.ObjectId
                            WHERE seo.ObjectType = 'Category'
                        INSERT INTO [CatalogSeoInfo] ([Id], [CreatedDate], [ModifiedDate], [CreatedBy], [ModifiedBy], [Keyword], [StoreId], [IsActive], [Language], [Title], [MetaDescription], [MetaKeywords], [ImageAltDescription], [ItemId])
                            SELECT seo.[Id], seo.[CreatedDate], seo.[ModifiedDate], seo.[CreatedBy], seo.[ModifiedBy], seo.[Keyword], seo.[StoreId], seo.[IsActive], seo.[Language], seo.[Title], seo.[MetaDescription], seo.[MetaKeywords], seo.[ImageAltDescription], seo.[ObjectId] as ItemId 
                            FROM [SeoUrlKeyword] seo
                            INNER JOIN Item ON Item.Id = seo.ObjectId
                            WHERE seo.ObjectType = 'CatalogProduct'
				    END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
