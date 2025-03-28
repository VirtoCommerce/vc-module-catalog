using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Repositories
{
    public class CatalogDbContext : DbContextBase
    {
#pragma warning disable S109
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
            : base(options)
        {
        }

        protected CatalogDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Catalog

            modelBuilder.Entity<CatalogEntity>().ToTable("Catalog").HasKey(x => x.Id);
            modelBuilder.Entity<CatalogEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();

            #endregion Catalog

            #region Category

            modelBuilder.Entity<CategoryEntity>().ToTable("Category");
            modelBuilder.Entity<CategoryEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<CategoryEntity>().HasOne(x => x.ParentCategory)
                .WithMany().HasForeignKey(x => x.ParentCategoryId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CategoryEntity>().HasOne(x => x.Catalog)
                .WithMany().HasForeignKey(x => x.CatalogId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CategoryEntity>().Property(x => x.ExcludedProperties).HasConversion(
                x => x != null && x.Count > 0 ? JsonConvert.SerializeObject(x) : null,
                x => x != null ? JsonConvert.DeserializeObject<List<string>>(x) : null,
                new ValueComparer<IList<string>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList())); // take a reference about value comparers here: https://learn.microsoft.com/en-us/ef/core/modeling/value-comparers?tabs=ef5#mutable-classes
            modelBuilder.Entity<CategoryEntity>().ToTable(t =>
                t.HasCheckConstraint("Parent_category_check", $"\"{nameof(CategoryEntity.ParentCategoryId)}\" != \"{nameof(CategoryEntity.Id)}\""));

            modelBuilder.Entity<CategoryLocalizedNameEntity>(builder =>
            {
                builder.ToTable("CategoryLocalizedName").HasKey(x => x.Id);
                builder.Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();

                builder.HasOne(x => x.ParentEntity)
                    .WithMany(x => x.LocalizedNames)
                    .HasForeignKey(x => x.ParentEntityId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasIndex(x => new { x.LanguageCode, x.ParentEntityId }).IsUnique()
                    .HasDatabaseName("IX_CategoryLocalizedName_LanguageCode_ParentEntityId");
            });
            #endregion Category

            #region Item

            modelBuilder.Entity<ItemEntity>().ToTable("Item").HasKey(x => x.Id);
            modelBuilder.Entity<ItemEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<ItemEntity>().Property(x => x.Weight).HasPrecision(18, 4);
            modelBuilder.Entity<ItemEntity>().Property(x => x.Height).HasPrecision(18, 4);
            modelBuilder.Entity<ItemEntity>().Property(x => x.Length).HasPrecision(18, 4);
            modelBuilder.Entity<ItemEntity>().Property(x => x.Width).HasPrecision(18, 4);
            modelBuilder.Entity<ItemEntity>().Property(x => x.MaxQuantity).HasPrecision(18, 2);
            modelBuilder.Entity<ItemEntity>().Property(x => x.MinQuantity).HasPrecision(18, 2);
            modelBuilder.Entity<ItemEntity>().HasOne(m => m.Catalog).WithMany().HasForeignKey(x => x.CatalogId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ItemEntity>().HasOne(m => m.Category).WithMany().HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ItemEntity>().HasOne(m => m.Parent).WithMany(x => x.Childrens).HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ItemEntity>().HasIndex(x => new { x.Code, x.CatalogId }).HasDatabaseName("IX_Code_CatalogId").IsUnique();
            modelBuilder.Entity<ItemEntity>().HasIndex(x => new { x.CatalogId, x.ParentId }).IsUnique(false).HasDatabaseName("IX_CatalogId_ParentId");
            modelBuilder.Entity<ItemEntity>().HasIndex(x => new { x.CreatedDate, x.ParentId }).IsUnique(false).HasDatabaseName("IX_CreatedDate_ParentId");

            modelBuilder.Entity<ItemEntity>().Property(x => x.PackSize).HasDefaultValue(1);

            modelBuilder.Entity<ItemLocalizedNameEntity>(builder =>
            {
                builder.ToTable("ItemLocalizedName").HasKey(x => x.Id);
                builder.Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();

                builder.HasOne(x => x.ParentEntity)
                    .WithMany(x => x.LocalizedNames)
                    .HasForeignKey(x => x.ParentEntityId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasIndex(x => new { x.LanguageCode, x.ParentEntityId }).IsUnique()
                    .HasDatabaseName("IX_ItemLocalizedName_LanguageCode_ParentEntityId");
            });

            #endregion Item

            #region Property

            modelBuilder.Entity<PropertyEntity>().ToTable("Property").HasKey(x => x.Id);
            modelBuilder.Entity<PropertyEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<PropertyEntity>().HasOne(m => m.Catalog).WithMany(x => x.Properties).HasForeignKey(x => x.CatalogId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PropertyEntity>().HasOne(m => m.Category).WithMany(x => x.Properties).HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion Property

            #region PropertyDictionaryItem

            modelBuilder.Entity<PropertyDictionaryItemEntity>().ToTable("PropertyDictionaryItem").HasKey(x => x.Id);
            modelBuilder.Entity<PropertyDictionaryItemEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<PropertyDictionaryItemEntity>().HasOne(m => m.Property).WithMany(p => p.DictionaryItems)
                .HasForeignKey(x => x.PropertyId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PropertyDictionaryItemEntity>().HasIndex(x => new { x.Alias, x.PropertyId })
                .IsUnique()
                .HasDatabaseName("IX_AliasAndPropertyId");

            #endregion PropertyDictionaryItem

            #region PropertyDictionaryValue

            modelBuilder.Entity<PropertyDictionaryValueEntity>().ToTable("PropertyDictionaryValue").HasKey(x => x.Id);
            modelBuilder.Entity<PropertyDictionaryValueEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<PropertyDictionaryValueEntity>().HasOne(m => m.DictionaryItem).WithMany(x => x.DictionaryItemValues)
                .HasForeignKey(x => x.DictionaryItemId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            #endregion PropertyDictionaryValue

            #region PropertyAttribute

            modelBuilder.Entity<PropertyAttributeEntity>().ToTable("PropertyAttribute").HasKey(x => x.Id);
            modelBuilder.Entity<PropertyAttributeEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<PropertyAttributeEntity>().HasOne(m => m.Property).WithMany(x => x.PropertyAttributes).HasForeignKey(x => x.PropertyId)
                .IsRequired().OnDelete(DeleteBehavior.Cascade);

            #endregion PropertyAttribute

            #region PropertyDisplayName

            modelBuilder.Entity<PropertyDisplayNameEntity>().ToTable("PropertyDisplayName").HasKey(x => x.Id);
            modelBuilder.Entity<PropertyDisplayNameEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<PropertyDisplayNameEntity>().HasOne(m => m.Property).WithMany(x => x.DisplayNames)
                .HasForeignKey(x => x.PropertyId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            #endregion PropertyDisplayName

            #region PropertyValue

            modelBuilder.Entity<PropertyValueEntity>().ToTable("PropertyValue").HasKey(x => x.Id);
            modelBuilder.Entity<PropertyValueEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<PropertyValueEntity>().Property(x => x.DecimalValue).HasPrecision(18, 4);
            modelBuilder.Entity<PropertyValueEntity>().HasOne(m => m.CatalogItem).WithMany(x => x.ItemPropertyValues)
                .HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PropertyValueEntity>().HasOne(m => m.Category).WithMany(x => x.CategoryPropertyValues)
                .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PropertyValueEntity>().HasOne(m => m.Catalog).WithMany(x => x.CatalogPropertyValues)
                .HasForeignKey(x => x.CatalogId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PropertyValueEntity>().HasOne(m => m.DictionaryItem).WithMany()
                .HasForeignKey(x => x.DictionaryItemId).OnDelete(DeleteBehavior.Cascade);

            #endregion PropertyValue

            #region PropertyValidationRule

            modelBuilder.Entity<PropertyValidationRuleEntity>().ToTable("PropertyValidationRule").HasKey(x => x.Id);
            modelBuilder.Entity<PropertyValidationRuleEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<PropertyValidationRuleEntity>().HasOne(m => m.Property).WithMany(x => x.ValidationRules).HasForeignKey(x => x.PropertyId)
                .IsRequired().OnDelete(DeleteBehavior.Cascade);

            #endregion PropertyValidationRule

            #region CatalogImage

            modelBuilder.Entity<ImageEntity>().ToTable("CatalogImage").HasKey(x => x.Id);
            modelBuilder.Entity<ImageEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<ImageEntity>().HasOne(m => m.Category).WithMany(x => x.Images)
                .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ImageEntity>().HasOne(m => m.CatalogItem).WithMany(x => x.Images)
                .HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Cascade);

            #endregion CatalogImage

            #region Video

            modelBuilder.Entity<VideoEntity>().ToTable("CatalogVideo").HasKey(x => x.Id);
            modelBuilder.Entity<VideoEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<VideoEntity>()
                .HasIndex(x => new { x.OwnerType, x.OwnerId })
                .IsUnique(false)
                .HasDatabaseName("IX_OwnerType_OwnerId");

            #endregion

            #region EditorialReview

            modelBuilder.Entity<EditorialReviewEntity>().ToTable("EditorialReview").HasKey(x => x.Id);
            modelBuilder.Entity<EditorialReviewEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<EditorialReviewEntity>().HasOne(x => x.CatalogItem).WithMany(x => x.EditorialReviews)
                .HasForeignKey(x => x.ItemId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            #endregion EditorialReview

            #region CategoryDescription

            modelBuilder.Entity<CategoryDescriptionEntity>().ToTable("CategoryDescription").HasKey(x => x.Id);
            modelBuilder.Entity<CategoryDescriptionEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<CategoryDescriptionEntity>().HasOne(x => x.Category).WithMany(x => x.CategoryDescriptions)
                .HasForeignKey(x => x.CategoryId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            #endregion CategoryDescription

            #region Association

            modelBuilder.Entity<AssociationEntity>().ToTable("Association").HasKey(x => x.Id);
            modelBuilder.Entity<AssociationEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<AssociationEntity>().HasOne(m => m.Item).WithMany(x => x.Associations)
                .HasForeignKey(x => x.ItemId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AssociationEntity>().HasOne(a => a.AssociatedItem).WithMany(i => i.ReferencedAssociations)
                .HasForeignKey(a => a.AssociatedItemId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AssociationEntity>().HasOne(a => a.AssociatedCategory).WithMany()
                .HasForeignKey(a => a.AssociatedCategoryId).OnDelete(DeleteBehavior.Restrict);

            #endregion Association

            #region Asset

            modelBuilder.Entity<AssetEntity>().ToTable("CatalogAsset").HasKey(x => x.Id);
            modelBuilder.Entity<AssetEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<AssetEntity>().HasOne(m => m.CatalogItem).WithMany(x => x.Assets).HasForeignKey(x => x.ItemId)
                .IsRequired().OnDelete(DeleteBehavior.Cascade);

            #endregion Asset

            #region CatalogLanguage

            modelBuilder.Entity<CatalogLanguageEntity>().ToTable("CatalogLanguage").HasKey(x => x.Id);
            modelBuilder.Entity<CatalogLanguageEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<CatalogLanguageEntity>().HasOne(m => m.Catalog).WithMany(x => x.CatalogLanguages)
                .HasForeignKey(x => x.CatalogId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            #endregion CatalogLanguage

            #region CategoryItemRelation

            modelBuilder.Entity<CategoryItemRelationEntity>().ToTable("CategoryItemRelation").HasKey(x => x.Id);
            modelBuilder.Entity<CategoryItemRelationEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<CategoryItemRelationEntity>().HasOne(p => p.Category).WithMany()
                .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CategoryItemRelationEntity>().HasOne(p => p.CatalogItem).WithMany(x => x.CategoryLinks).HasForeignKey(x => x.ItemId)
                .IsRequired().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CategoryItemRelationEntity>().HasOne(p => p.Catalog).WithMany().HasForeignKey(x => x.CatalogId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);

            #endregion CategoryItemRelation

            #region CategoryRelation

            modelBuilder.Entity<CategoryRelationEntity>().ToTable("CategoryRelation").HasKey(x => x.Id);
            modelBuilder.Entity<CategoryRelationEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();

            modelBuilder.Entity<CategoryRelationEntity>().HasOne(x => x.TargetCategory).WithMany(x => x.IncomingLinks)
                .HasForeignKey(x => x.TargetCategoryId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CategoryRelationEntity>().HasOne(x => x.SourceCategory).WithMany(x => x.OutgoingLinks)
                .HasForeignKey(x => x.SourceCategoryId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CategoryRelationEntity>().HasOne(x => x.TargetCatalog)
                .WithMany(x => x.IncomingLinks)
                .HasForeignKey(x => x.TargetCatalogId).OnDelete(DeleteBehavior.Restrict);

            #endregion CategoryRelation

            #region SeoInfo

            modelBuilder.Entity<SeoInfoEntity>().ToTable("CatalogSeoInfo").HasKey(x => x.Id);
            modelBuilder.Entity<SeoInfoEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<SeoInfoEntity>().HasOne(x => x.Category).WithMany(x => x.SeoInfos).HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SeoInfoEntity>().HasOne(x => x.Catalog).WithMany(x => x.SeoInfos).HasForeignKey(x => x.CatalogId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SeoInfoEntity>().HasOne(x => x.Item).WithMany(x => x.SeoInfos).HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion SeoInfo

            #region Measure

            modelBuilder.Entity<MeasureEntity>().ToTable("Measure").HasKey(x => x.Id);
            modelBuilder.Entity<MeasureEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();

            #endregion

            #region MeasureUnit

            modelBuilder.Entity<MeasureUnitEntity>().ToTable("MeasureUnit").HasKey(x => x.Id);
            modelBuilder.Entity<MeasureUnitEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<MeasureUnitEntity>().Property(x => x.ConversionFactor).HasPrecision(21, 6);
            modelBuilder.Entity<MeasureUnitEntity>().HasOne(x => x.Measure).WithMany(x => x.Units).HasForeignKey(x => x.MeasureId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MeasureUnitLocalizedNameEntity>(builder =>
            {
                builder.ToTable("MeasureUnitLocalizedName").HasKey(x => x.Id);
                builder.Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();

                builder.HasOne(x => x.ParentEntity)
                    .WithMany(x => x.LocalizedNames)
                    .HasForeignKey(x => x.ParentEntityId)
                    .IsRequired();

                builder.HasIndex(x => new { x.LanguageCode, x.ParentEntityId }).IsUnique()
                    .HasDatabaseName("IX_MeasureUnitLocalizedName_LanguageCode_ParentEntityId");
            });

            modelBuilder.Entity<MeasureUnitLocalizedSymbolEntity>(builder =>
            {
                builder.ToTable("MeasureUnitLocalizedSymbol").HasKey(x => x.Id);
                builder.Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();

                builder.HasOne(x => x.ParentEntity)
                    .WithMany(x => x.LocalizedSymbols)
                    .HasForeignKey(x => x.ParentEntityId)
                    .IsRequired();

                builder.HasIndex(x => new { x.LanguageCode, x.ParentEntityId }).IsUnique()
                    .HasDatabaseName("IX_MeasureUnitLocalizedSymbol_LanguageCode_ParentEntityId");
            });

            #endregion

            #region ProductConfiguration

            modelBuilder.Entity<ProductConfigurationEntity>().ToTable("ProductConfiguration").HasKey(x => x.Id);
            modelBuilder.Entity<ProductConfigurationEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<ProductConfigurationEntity>().HasOne(x => x.Product).WithOne()
                .HasForeignKey<ProductConfigurationEntity>(x => x.ProductId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductConfigurationSectionEntity>().ToTable("ProductConfigurationSection").HasKey(x => x.Id);
            modelBuilder.Entity<ProductConfigurationSectionEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<ProductConfigurationSectionEntity>().HasOne(x => x.Configuration).WithMany(x => x.Sections)
                .HasForeignKey(x => x.ConfigurationId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductConfigurationOptionEntity>().ToTable("ProductConfigurationOption").HasKey(x => x.Id);
            modelBuilder.Entity<ProductConfigurationOptionEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<ProductConfigurationOptionEntity>().HasOne(x => x.Section).WithMany(x => x.Options)
                .HasForeignKey(x => x.SectionId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ProductConfigurationOptionEntity>().HasOne(x => x.Product).WithMany()
                .HasForeignKey(x => x.ProductId).IsRequired().OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<ProductConfigurationOptionEntity>().Property(x => x.Quantity).HasDefaultValue(1);

            #endregion

            base.OnModelCreating(modelBuilder);

            // Allows configuration for an entity type for different database types.
            // Applies configuration from all <see cref="IEntityTypeConfiguration{TEntity}" in VirtoCommerce.CatalogModule.Data.XXX project. /> 
            switch (Database.ProviderName)
            {
                case "Pomelo.EntityFrameworkCore.MySql":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.CatalogModule.Data.MySql"));
                    break;
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.CatalogModule.Data.PostgreSql"));
                    break;
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.CatalogModule.Data.SqlServer"));
                    break;
            }

        }
    }
#pragma warning restore S109
}
