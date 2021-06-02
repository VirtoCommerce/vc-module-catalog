using System.Collections.Generic;
using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using VirtoCommerce.CatalogModule.Data.Model;

namespace VirtoCommerce.CatalogModule.Data.Repositories
{
    public class CatalogDbContext : DbContextWithTriggers
    {
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
                x => x != null ? JsonConvert.DeserializeObject<List<string>>(x) : null);

            #endregion Category

            #region Item

            modelBuilder.Entity<ItemEntity>().ToTable("Item").HasKey(x => x.Id);
            modelBuilder.Entity<ItemEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<ItemEntity>().HasOne(m => m.Catalog).WithMany().HasForeignKey(x => x.CatalogId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ItemEntity>().HasOne(m => m.Category).WithMany().HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ItemEntity>().HasOne(m => m.Parent).WithMany(x => x.Childrens).HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ItemEntity>().HasIndex(x => new { x.Code, x.CatalogId }).HasName("IX_Code_CatalogId").IsUnique();
            modelBuilder.Entity<ItemEntity>().HasIndex(x => new { x.CatalogId, x.ParentId }).IsUnique(false).HasName("IX_CatalogId_ParentId");
            modelBuilder.Entity<ItemEntity>().HasIndex(x => new { x.CreatedDate, x.ParentId }).IncludeProperties(x => x.ModifiedDate).IsUnique(false).HasName("IX_CreatedDate_ParentId");

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
                .HasName("IX_AliasAndPropertyId");

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

            #region EditorialReview

            modelBuilder.Entity<EditorialReviewEntity>().ToTable("EditorialReview").HasKey(x => x.Id);
            modelBuilder.Entity<EditorialReviewEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<EditorialReviewEntity>().HasOne(x => x.CatalogItem).WithMany(x => x.EditorialReviews)
                .HasForeignKey(x => x.ItemId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            #endregion EditorialReview

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
            modelBuilder.Entity<SeoInfoEntity>().HasOne(x => x.Item).WithMany(x => x.SeoInfos).HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion SeoInfo

            base.OnModelCreating(modelBuilder);
        }
    }
}
