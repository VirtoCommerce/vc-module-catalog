using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.CatalogModule.Data.Repositories
{
    public class CatalogRepositoryImpl : EFRepositoryBase, ICatalogRepository
    {
        public CatalogRepositoryImpl()
            : base()
        {
        }

        public CatalogRepositoryImpl(string nameOrConnectionString, params IInterceptor[] interceptors)
            : base(nameOrConnectionString, null, interceptors)
        {
            Database.SetInitializer<CatalogRepositoryImpl>(null);
        }

        public CatalogRepositoryImpl(DbConnection existingConnection, IUnitOfWork unitOfWork = null,
            IInterceptor[] interceptors = null) : base(existingConnection, unitOfWork, interceptors)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            #region Catalog
            modelBuilder.Entity<CatalogEntity>().ToTable("Catalog").HasKey(x => x.Id).Property(x => x.Id);
            #endregion

            #region Category
            modelBuilder.Entity<CategoryEntity>().ToTable("Category").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<CategoryEntity>().HasOptional(x => x.ParentCategory).WithMany().HasForeignKey(x => x.ParentCategoryId).WillCascadeOnDelete(false);
            modelBuilder.Entity<CategoryEntity>().HasRequired(x => x.Catalog).WithMany().HasForeignKey(x => x.CatalogId).WillCascadeOnDelete(true);
            #endregion

            #region Item
            modelBuilder.Entity<ItemEntity>().ToTable("Item").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<ItemEntity>().HasRequired(m => m.Catalog).WithMany().HasForeignKey(x => x.CatalogId).WillCascadeOnDelete(false);
            modelBuilder.Entity<ItemEntity>().HasOptional(m => m.Category).WithMany().HasForeignKey(x => x.CategoryId).WillCascadeOnDelete(false);
            modelBuilder.Entity<ItemEntity>().HasOptional(m => m.Parent).WithMany(x => x.Childrens).HasForeignKey(x => x.ParentId).WillCascadeOnDelete(false);
            #endregion

            #region Property
            modelBuilder.Entity<PropertyEntity>().ToTable("Property").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<PropertyEntity>().HasOptional(m => m.Catalog).WithMany(x => x.Properties).HasForeignKey(x => x.CatalogId).WillCascadeOnDelete(false);
            modelBuilder.Entity<PropertyEntity>().HasOptional(m => m.Category).WithMany(x => x.Properties).HasForeignKey(x => x.CategoryId).WillCascadeOnDelete(false);

            #endregion

            #region PropertyDictionaryItem
            modelBuilder.Entity<PropertyDictionaryItemEntity>().ToTable("PropertyDictionaryItem").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<PropertyDictionaryItemEntity>().HasRequired(m => m.Property).WithMany(x => x.DictionaryItems).HasForeignKey(x => x.PropertyId).WillCascadeOnDelete(true);
            #endregion

            #region PropertyDictionaryValue
            modelBuilder.Entity<PropertyDictionaryValueEntity>().ToTable("PropertyDictionaryValue").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<PropertyDictionaryValueEntity>().HasRequired(m => m.DictionaryItem).WithMany(x => x.DictionaryItemValues).HasForeignKey(x => x.DictionaryItemId).WillCascadeOnDelete(true);
            #endregion

            #region PropertyAttribute
            modelBuilder.Entity<PropertyAttributeEntity>().ToTable("PropertyAttribute").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<PropertyAttributeEntity>().HasRequired(m => m.Property).WithMany(x => x.PropertyAttributes).HasForeignKey(x => x.PropertyId).WillCascadeOnDelete(true);
            #endregion

            #region PropertyDisplayName
            modelBuilder.Entity<PropertyDisplayNameEntity>().ToTable("PropertyDisplayName").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<PropertyDisplayNameEntity>().HasRequired(m => m.Property).WithMany(x => x.DisplayNames).HasForeignKey(x => x.PropertyId).WillCascadeOnDelete(true);
            #endregion

            #region PropertyValue
            modelBuilder.Entity<PropertyValueEntity>().ToTable("PropertyValue").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<PropertyValueEntity>().HasOptional(m => m.CatalogItem).WithMany(x => x.ItemPropertyValues).HasForeignKey(x => x.ItemId).WillCascadeOnDelete(false);
            modelBuilder.Entity<PropertyValueEntity>().HasOptional(m => m.Category).WithMany(x => x.CategoryPropertyValues).HasForeignKey(x => x.CategoryId).WillCascadeOnDelete(false);
            modelBuilder.Entity<PropertyValueEntity>().HasOptional(m => m.Catalog).WithMany(x => x.CatalogPropertyValues).HasForeignKey(x => x.CatalogId).WillCascadeOnDelete(false);
            modelBuilder.Entity<PropertyValueEntity>().HasOptional(m => m.DictionaryItem).WithMany().HasForeignKey(x => x.DictionaryItemId).WillCascadeOnDelete(true);
            #endregion

            #region PropertyValidationRule
            modelBuilder.Entity<PropertyValidationRuleEntity>().ToTable("PropertyValidationRule").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<PropertyValidationRuleEntity>().HasRequired(m => m.Property).WithMany(x => x.ValidationRules).HasForeignKey(x => x.PropertyId).WillCascadeOnDelete(true);

            #endregion

            #region CatalogImage
            modelBuilder.Entity<ImageEntity>().ToTable("CatalogImage").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<ImageEntity>().HasOptional(m => m.Category).WithMany(x => x.Images).HasForeignKey(x => x.CategoryId).WillCascadeOnDelete(false);
            modelBuilder.Entity<ImageEntity>().HasOptional(m => m.CatalogItem).WithMany(x => x.Images).HasForeignKey(x => x.ItemId).WillCascadeOnDelete(false);
            #endregion

            #region EditorialReview
            modelBuilder.Entity<EditorialReviewEntity>().ToTable("EditorialReview").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<EditorialReviewEntity>().HasRequired(x => x.CatalogItem).WithMany(x => x.EditorialReviews).HasForeignKey(x => x.ItemId).WillCascadeOnDelete(true);
            #endregion         

            #region Association
            modelBuilder.Entity<AssociationEntity>().ToTable("Association").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<AssociationEntity>().HasRequired(m => m.Item).WithMany(x => x.Associations).HasForeignKey(x => x.ItemId).WillCascadeOnDelete(false);
            modelBuilder.Entity<AssociationEntity>().HasOptional(a => a.AssociatedItem).WithMany(i => i.ReferencedAssociations).HasForeignKey(a => a.AssociatedItemId).WillCascadeOnDelete(false);
            modelBuilder.Entity<AssociationEntity>().HasOptional(a => a.AssociatedCategory).WithMany().HasForeignKey(a => a.AssociatedCategoryId).WillCascadeOnDelete(false);
            #endregion

            #region Asset
            modelBuilder.Entity<AssetEntity>().ToTable("CatalogAsset").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<AssetEntity>().HasRequired(m => m.CatalogItem).WithMany(x => x.Assets).HasForeignKey(x => x.ItemId).WillCascadeOnDelete(true);
            #endregion

            #region CatalogLanguage
            modelBuilder.Entity<CatalogLanguageEntity>().ToTable("CatalogLanguage").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<CatalogLanguageEntity>().HasRequired(m => m.Catalog).WithMany(x => x.CatalogLanguages).HasForeignKey(x => x.CatalogId).WillCascadeOnDelete(true);
            #endregion

            #region CategoryItemRelation
            modelBuilder.Entity<CategoryItemRelationEntity>().ToTable("CategoryItemRelation").HasKey(x => x.Id).Property(x => x.Id);

            modelBuilder.Entity<CategoryItemRelationEntity>().HasOptional(p => p.Category).WithMany().HasForeignKey(x => x.CategoryId).WillCascadeOnDelete(false);
            modelBuilder.Entity<CategoryItemRelationEntity>().HasRequired(p => p.CatalogItem).WithMany(x => x.CategoryLinks).HasForeignKey(x => x.ItemId).WillCascadeOnDelete(false);
            modelBuilder.Entity<CategoryItemRelationEntity>().HasRequired(p => p.Catalog).WithMany().HasForeignKey(x => x.CatalogId).WillCascadeOnDelete(false);
            #endregion

            #region CategoryRelation
            modelBuilder.Entity<CategoryRelationEntity>().ToTable("CategoryRelation").HasKey(x => x.Id).Property(x => x.Id);

            modelBuilder.Entity<CategoryRelationEntity>().HasOptional(x => x.TargetCategory)
                                       .WithMany(x => x.IncommingLinks)
                                       .HasForeignKey(x => x.TargetCategoryId).WillCascadeOnDelete(false);

            modelBuilder.Entity<CategoryRelationEntity>().HasRequired(x => x.SourceCategory)
                                       .WithMany(x => x.OutgoingLinks)
                                       .HasForeignKey(x => x.SourceCategoryId).WillCascadeOnDelete(false);

            modelBuilder.Entity<CategoryRelationEntity>().HasOptional(x => x.TargetCatalog)
                                       .WithMany(x => x.IncommingLinks)
                                       .HasForeignKey(x => x.TargetCatalogId).WillCascadeOnDelete(false);
            #endregion

            base.OnModelCreating(modelBuilder);
        }

        #region ICatalogRepository Members

        public IQueryable<CategoryEntity> Categories => GetAsQueryable<CategoryEntity>();
        public IQueryable<CatalogEntity> Catalogs => GetAsQueryable<CatalogEntity>();
        public IQueryable<PropertyValueEntity> PropertyValues => GetAsQueryable<PropertyValueEntity>();
        public IQueryable<ImageEntity> Images => GetAsQueryable<ImageEntity>();
        public IQueryable<AssetEntity> Assets => GetAsQueryable<AssetEntity>();
        public IQueryable<ItemEntity> Items => GetAsQueryable<ItemEntity>();
        public IQueryable<EditorialReviewEntity> EditorialReviews => GetAsQueryable<EditorialReviewEntity>();
        public IQueryable<PropertyEntity> Properties => GetAsQueryable<PropertyEntity>();
        public IQueryable<PropertyDictionaryValueEntity> PropertyDictionaryValues => GetAsQueryable<PropertyDictionaryValueEntity>();
        public IQueryable<PropertyDictionaryItemEntity> PropertyDictionaryItems => GetAsQueryable<PropertyDictionaryItemEntity>();
        public IQueryable<PropertyDisplayNameEntity> PropertyDisplayNames => GetAsQueryable<PropertyDisplayNameEntity>();
        public IQueryable<PropertyAttributeEntity> PropertyAttributes => GetAsQueryable<PropertyAttributeEntity>();
        public IQueryable<CategoryItemRelationEntity> CategoryItemRelations => GetAsQueryable<CategoryItemRelationEntity>();
        public IQueryable<AssociationEntity> Associations => GetAsQueryable<AssociationEntity>();
        public IQueryable<CategoryRelationEntity> CategoryLinks => GetAsQueryable<CategoryRelationEntity>();
        public IQueryable<PropertyValidationRuleEntity> PropertyValidationRules => GetAsQueryable<PropertyValidationRuleEntity>();

        public CatalogEntity[] GetCatalogsByIds(string[] catalogIds)
        {
            var result = Array.Empty<CatalogEntity>();

            if (!catalogIds.IsNullOrEmpty())
            {
                result = Catalogs.Include(x => x.CatalogLanguages)
                    .Include(x => x.IncommingLinks)
                    .Where(x => catalogIds.Contains(x.Id))
                    .ToArray();

                if (result.Any())
                {
                    catalogIds = result.Select(x => x.Id).ToArray();

                    var propertyValues = PropertyValues.Include(x => x.DictionaryItem.DictionaryItemValues).Where(x => catalogIds.Contains(x.CatalogId) && x.CategoryId == null).ToArray();
                    var catalogPropertiesIds = Properties.Where(x => catalogIds.Contains(x.CatalogId) && x.CategoryId == null)
                        .Select(x => x.Id)
                        .ToArray();
                    var catalogProperties = GetPropertiesByIds(catalogPropertiesIds);
                }
            }

            return result;
        }

        public CategoryEntity[] GetCategoriesByIds(string[] categoriesIds, CategoryResponseGroup respGroup)
        {
            var result = Array.Empty<CategoryEntity>();

            if (!categoriesIds.IsNullOrEmpty())
            {
                if (respGroup.HasFlag(CategoryResponseGroup.WithOutlines))
                {
                    respGroup |= CategoryResponseGroup.WithLinks | CategoryResponseGroup.WithParents;
                }

                result = Categories.Where(x => categoriesIds.Contains(x.Id)).ToArray();

                if (result.Any())
                {
                    categoriesIds = result.Select(x => x.Id).ToArray();

                    if (respGroup.HasFlag(CategoryResponseGroup.WithLinks))
                    {
                        var incommingLinks = CategoryLinks.Where(x => categoriesIds.Contains(x.TargetCategoryId)).ToArray();
                        var outgoingLinks = CategoryLinks.Where(x => categoriesIds.Contains(x.SourceCategoryId)).ToArray();
                    }

                    if (respGroup.HasFlag(CategoryResponseGroup.WithImages))
                    {
                        var images = Images.Where(x => categoriesIds.Contains(x.CategoryId)).ToArray();
                    }

                    //Load all properties meta information and information for inheritance
                    if (respGroup.HasFlag(CategoryResponseGroup.WithProperties))
                    {
                        //Load category property values by separate query
                        var propertyValues = PropertyValues.Include(x => x.DictionaryItem.DictionaryItemValues).Where(x => categoriesIds.Contains(x.CategoryId)).ToArray();

                        var categoryPropertiesIds = Properties.Where(x => categoriesIds.Contains(x.CategoryId)).Select(x => x.Id).ToArray();
                        var categoryProperties = GetPropertiesByIds(categoryPropertiesIds);
                    }
                }
            }

            return result;
        }

        public ItemEntity[] GetItemByIds(string[] itemIds, ItemResponseGroup respGroup = ItemResponseGroup.ItemLarge)
        {
            var result = Array.Empty<ItemEntity>();

            if (!itemIds.IsNullOrEmpty())
            {
                result = Items.Include(x => x.Images).Where(x => itemIds.Contains(x.Id)).ToArray();

                if (result.Any())
                {
                    itemIds = result.Select(x => x.Id).ToArray();

                    if (respGroup.HasFlag(ItemResponseGroup.Outlines))
                    {
                        respGroup |= ItemResponseGroup.Links;
                    }

                    if (respGroup.HasFlag(ItemResponseGroup.ItemProperties))
                    {
                        var propertyValues = PropertyValues.Include(x => x.DictionaryItem.DictionaryItemValues).Where(x => itemIds.Contains(x.ItemId)).ToArray();
                    }

                    if (respGroup.HasFlag(ItemResponseGroup.Links))
                    {
                        var relations = CategoryItemRelations.Where(x => itemIds.Contains(x.ItemId)).ToArray();
                    }

                    if (respGroup.HasFlag(ItemResponseGroup.ItemAssets))
                    {
                        var assets = Assets.Where(x => itemIds.Contains(x.ItemId)).ToArray();
                    }

                    if (respGroup.HasFlag(ItemResponseGroup.ItemEditorialReviews))
                    {
                        var editorialReviews = EditorialReviews.Where(x => itemIds.Contains(x.ItemId)).ToArray();
                    }

                    if (respGroup.HasFlag(ItemResponseGroup.Variations))
                    {
                        // TODO: Call GetItemByIds for variations recursively (need to measure performance and data amount first)

                        var variationIds = Items.Where(x => itemIds.Contains(x.ParentId)).Select(x => x.Id).ToArray();

                        if (!variationIds.IsNullOrEmpty())
                        {
                            // Always load info, images and property values for variations
                            var variations = Items.Include(x => x.Images).Where(x => variationIds.Contains(x.Id)).ToArray();

                            if (variations.Any())
                            {
                                variationIds = variations.Select(x => x.Id).ToArray();

                                var variationPropertyValues = PropertyValues.Include(x => x.DictionaryItem.DictionaryItemValues).Where(x => variationIds.Contains(x.ItemId)).ToArray();

                                if (respGroup.HasFlag(ItemResponseGroup.ItemAssets))
                                {
                                    var variationAssets = Assets.Where(x => variationIds.Contains(x.ItemId)).ToArray();
                                }

                                if (respGroup.HasFlag(ItemResponseGroup.ItemEditorialReviews))
                                {
                                    var variationEditorialReviews = EditorialReviews.Where(x => variationIds.Contains(x.ItemId)).ToArray();
                                }
                            }
                        }
                    }

                    if (respGroup.HasFlag(ItemResponseGroup.ItemAssociations))
                    {
                        var assosiations = Associations.Where(x => itemIds.Contains(x.ItemId)).ToArray();
                        var assosiatedProductIds = assosiations.Where(x => x.AssociatedItemId != null)
                                                               .Select(x => x.AssociatedItemId).Distinct().ToArray();

                        var assosiatedItems = GetItemByIds(assosiatedProductIds, ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemAssets);

                        var assosiatedCategoryIdsIds = assosiations.Where(x => x.AssociatedCategoryId != null).Select(x => x.AssociatedCategoryId).Distinct().ToArray();
                        var associatedCategories = GetCategoriesByIds(assosiatedCategoryIdsIds, CategoryResponseGroup.Info | CategoryResponseGroup.WithImages);
                    }

                    if (respGroup.HasFlag(ItemResponseGroup.ReferencedAssociations))
                    {
                        var referencedAssociations = Associations.Where(x => itemIds.Contains(x.AssociatedItemId)).ToArray();
                        var referencedProductIds = referencedAssociations.Select(x => x.ItemId).Distinct().ToArray();
                        var referencedProducts = GetItemByIds(referencedProductIds, ItemResponseGroup.ItemInfo);
                    }

                    // Load parents
                    var parentIds = result.Where(x => x.Parent == null && x.ParentId != null).Select(x => x.ParentId).ToArray();
                    var parents = GetItemByIds(parentIds, respGroup);
                }
            }

            return result;
        }

        public PropertyEntity[] GetPropertiesByIds(string[] propIds, bool loadDictValues = false)
        {
            var result = Array.Empty<PropertyEntity>();

            if (!propIds.IsNullOrEmpty())
            {
                //Used breaking query EF performance concept https://msdn.microsoft.com/en-us/data/hh949853.aspx#8
                result = Properties.Where(x => propIds.Contains(x.Id)).ToArray();

                if (result.Any())
                {
                    propIds = result.Select(x => x.Id).ToArray();

                    var propAttributes = PropertyAttributes.Where(x => propIds.Contains(x.PropertyId)).ToArray();
                    var propDisplayNames = PropertyDisplayNames.Where(x => propIds.Contains(x.PropertyId)).ToArray();
                    var propValidationRules = PropertyValidationRules.Where(x => propIds.Contains(x.PropertyId)).ToArray();

                    if (loadDictValues)
                    {
                        var propDictionaryItems = PropertyDictionaryItems.Include(x => x.DictionaryItemValues).Where(x => propIds.Contains(x.PropertyId)).ToArray();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returned all properties belongs to specified catalog 
        /// For virtual catalog also include all properties for categories linked to this virtual catalog 
        /// </summary>
        /// <param name="catalogId"></param>
        /// <returns></returns>
        public PropertyEntity[] GetAllCatalogProperties(string catalogId)
        {
            var result = Array.Empty<PropertyEntity>();

            if (!catalogId.IsNullOrEmpty())
            {
                var catalog = Catalogs.FirstOrDefault(x => x.Id == catalogId);

                if (catalog != null)
                {
                    var propertyIds = Properties.Where(x => x.CatalogId == catalogId).Select(x => x.Id).ToArray();

                    if (catalog.Virtual)
                    {
                        //get all category relations
                        var linkedCategoryIds = CategoryLinks.Where(x => x.TargetCatalogId == catalogId)
                                                             .Select(x => x.SourceCategoryId)
                                                             .Distinct()
                                                             .ToArray();
                        //linked product categories links
                        var linkedProductCategoryIds = CategoryItemRelations.Where(x => x.CatalogId == catalogId)
                                                                 .Join(Items, link => link.ItemId, item => item.Id, (link, item) => item)
                                                                 .Select(x => x.CategoryId)
                                                                 .Distinct()
                                                                 .ToArray();
                        linkedCategoryIds = linkedCategoryIds.Concat(linkedProductCategoryIds).Distinct().ToArray();
                        var expandedFlatLinkedCategoryIds = linkedCategoryIds.Concat(GetAllChildrenCategoriesIds(linkedCategoryIds)).Distinct().ToArray();

                        if (expandedFlatLinkedCategoryIds.Any())
                        {
                            propertyIds = propertyIds.Concat(Properties.Where(x => expandedFlatLinkedCategoryIds.Contains(x.CategoryId)).Select(x => x.Id)).Distinct().ToArray();
                            var linkedCatalogIds = Categories.Where(x => expandedFlatLinkedCategoryIds.Contains(x.Id)).Select(x => x.CatalogId).Distinct().ToArray();

                            if (linkedCatalogIds.Any())
                            {
                                propertyIds = propertyIds.Concat(Properties.Where(x => linkedCatalogIds.Contains(x.CatalogId) && x.CategoryId == null).Select(x => x.Id)).Distinct().ToArray();
                            }
                        }
                    }

                    result = GetPropertiesByIds(propertyIds).ToArray();
                }
            }

            return result;
        }

        public string[] GetAllChildrenCategoriesIds(string[] categoryIds)
        {
            string[] result = null;

            if (!categoryIds.IsNullOrEmpty())
            {
                const string commandTemplate = @"
                    WITH cte AS (
                        SELECT a.Id FROM Category a  WHERE Id IN ({0})
                        UNION ALL
                        SELECT a.Id FROM Category a JOIN cte c ON a.ParentCategoryId = c.Id
                    )
                    SELECT Id FROM cte WHERE Id NOT IN ({0})
                ";

                result = ExecuteStoreQuery<string>(commandTemplate, categoryIds).ToArray();
            }

            return result ?? new string[0];
        }

        public void RemoveItems(string[] itemIds)
        {
            if (!itemIds.IsNullOrEmpty())
            {
                const string commandTemplate = @"
                    DELETE SEO FROM SeoUrlKeyword SEO INNER JOIN Item I ON I.Id = SEO.ObjectId AND SEO.ObjectType = 'CatalogProduct'
                    WHERE I.Id IN ({0}) OR I.ParentId IN ({0})

                    DELETE CR FROM CategoryItemRelation  CR INNER JOIN Item I ON I.Id = CR.ItemId
                    WHERE I.Id IN ({0}) OR I.ParentId IN ({0})
        
                    DELETE CI FROM CatalogImage CI INNER JOIN Item I ON I.Id = CI.ItemId
                    WHERE I.Id IN ({0})  OR I.ParentId IN ({0})

                    DELETE CA FROM CatalogAsset CA INNER JOIN Item I ON I.Id = CA.ItemId
                    WHERE I.Id IN ({0}) OR I.ParentId IN ({0})

                    DELETE PV FROM PropertyValue PV INNER JOIN Item I ON I.Id = PV.ItemId
                    WHERE I.Id IN ({0}) OR I.ParentId IN ({0})

                    DELETE ER FROM EditorialReview ER INNER JOIN Item I ON I.Id = ER.ItemId
                    WHERE I.Id IN ({0}) OR I.ParentId IN ({0})

                    DELETE A FROM Association A INNER JOIN Item I ON I.Id = A.ItemId
                    WHERE I.Id IN ({0}) OR I.ParentId IN ({0})

                    DELETE A FROM Association A INNER JOIN Item I ON I.Id = A.AssociatedItemId
                    WHERE I.Id IN ({0}) OR I.ParentId IN ({0})

                    DELETE  FROM Item  WHERE ParentId IN ({0})

                    DELETE  FROM Item  WHERE Id IN ({0})
                ";

                const int batchSize = 500;
                var skip = 0;

                do
                {
                    var batchItemIds = itemIds.Skip(skip).Take(batchSize).ToArray();
                    ExecuteStoreCommand(commandTemplate, batchItemIds);

                    skip += batchSize;
                }
                while (skip < itemIds.Length);

                AddBatchDeletedEntities<ItemEntity>(itemIds);
            }
        }

        public void RemoveCategories(string[] ids)
        {
            if (!ids.IsNullOrEmpty())
            {
                var categoryIds = GetAllChildrenCategoriesIds(ids).Concat(ids).ToArray();

                var itemIds = Items.Where(i => categoryIds.Contains(i.CategoryId)).Select(i => i.Id).ToArray();
                RemoveItems(itemIds);

                const string commandTemplate = @"
                    DELETE FROM SeoUrlKeyword WHERE ObjectType = 'Category' AND ObjectId IN ({0})
                    DELETE CI FROM CatalogImage CI INNER JOIN Category C ON C.Id = CI.CategoryId WHERE C.Id IN ({0}) 
                    DELETE PV FROM PropertyValue PV INNER JOIN Category C ON C.Id = PV.CategoryId WHERE C.Id IN ({0}) 
                    DELETE CR FROM CategoryRelation CR INNER JOIN Category C ON C.Id = CR.SourceCategoryId OR C.Id = CR.TargetCategoryId  WHERE C.Id IN ({0}) 
                    DELETE CIR FROM CategoryItemRelation CIR INNER JOIN Category C ON C.Id = CIR.CategoryId WHERE C.Id IN ({0}) 
                    DELETE A FROM Association A INNER JOIN Category C ON C.Id = A.AssociatedCategoryId WHERE C.Id IN ({0})
                    DELETE P FROM Property P INNER JOIN Category C ON C.Id = P.CategoryId  WHERE C.Id IN ({0})
                    DELETE FROM Category WHERE Id IN ({0})
                ";

                ExecuteStoreCommand(commandTemplate, categoryIds);

                AddBatchDeletedEntities<CategoryEntity>(ids);
            }
        }

        public void RemoveCatalogs(string[] ids)
        {
            if (!ids.IsNullOrEmpty())
            {
                var itemIds = Items.Where(i => i.CategoryId == null && ids.Contains(i.CatalogId)).Select(i => i.Id).ToArray();
                RemoveItems(itemIds);

                var categoryIds = Categories.Where(c => ids.Contains(c.CatalogId)).Select(c => c.Id).ToArray();
                RemoveCategories(categoryIds);

                const string commandTemplate = @"
                    DELETE CL FROM CatalogLanguage CL INNER JOIN Catalog C ON C.Id = CL.CatalogId WHERE C.Id IN ({0})
                    DELETE CR FROM CategoryRelation CR INNER JOIN Catalog C ON C.Id = CR.TargetCatalogId WHERE C.Id IN ({0}) 
                    DELETE PV FROM PropertyValue PV INNER JOIN Catalog C ON C.Id = PV.CatalogId WHERE C.Id IN ({0}) 
                    DELETE P FROM Property P INNER JOIN Catalog C ON C.Id = P.CatalogId  WHERE C.Id IN ({0})
                    DELETE FROM Catalog WHERE Id IN ({0})
                ";

                ExecuteStoreCommand(commandTemplate, ids);

                AddBatchDeletedEntities<CatalogEntity>(ids);
            }
        }

        /// <summary>
        /// Delete all exist property values belong to given property.
        /// Because PropertyValue table doesn't have a foreign key to Property table by design,
        /// we use columns Name and TargetType to find values that reference to the deleting property.  
        /// </summary>
        /// <param name="propertyId"></param>
        public void RemoveAllPropertyValues(string propertyId)
        {
            var properties = GetPropertiesByIds(new[] { propertyId });
            var catalogProperty = properties.FirstOrDefault(x => x.TargetType.EqualsInvariant(PropertyType.Catalog.ToString()));
            var categoryProperty = properties.FirstOrDefault(x => x.TargetType.EqualsInvariant(PropertyType.Category.ToString()));
            var itemProperty = properties.FirstOrDefault(x => x.TargetType.EqualsInvariant(PropertyType.Product.ToString()) || x.TargetType.EqualsInvariant(PropertyType.Variation.ToString()));

            string commandTemplate;
            if (catalogProperty != null)
            {
                commandTemplate = $"DELETE PV FROM PropertyValue PV INNER JOIN Catalog C ON C.Id = PV.CatalogId AND C.Id = '{catalogProperty.CatalogId}' WHERE PV.Name = '{catalogProperty.Name}'";
                ObjectContext.ExecuteStoreCommand(commandTemplate);
            }
            if (categoryProperty != null)
            {
                commandTemplate = $"DELETE PV FROM PropertyValue PV INNER JOIN Category C ON C.Id = PV.CategoryId AND C.CatalogId = '{categoryProperty.CatalogId}' WHERE PV.Name = '{categoryProperty.Name}'";
                ObjectContext.ExecuteStoreCommand(commandTemplate);
            }
            if (itemProperty != null)
            {
                commandTemplate = $"DELETE PV FROM PropertyValue PV INNER JOIN Item I ON I.Id = PV.ItemId AND I.CatalogId = '{itemProperty.CatalogId}' WHERE PV.Name = '{itemProperty.Name}'";
                ObjectContext.ExecuteStoreCommand(commandTemplate);
            }
        }

        public GenericSearchResult<AssociationEntity> SearchAssociations(ProductAssociationSearchCriteria criteria)
        {
            var result = new GenericSearchResult<AssociationEntity>();

            var countSqlCommandText = @"
                ;WITH Association_CTE AS
                (
	                SELECT *
	                FROM Association
	                WHERE ItemId IN ({0})
                "
                + (!string.IsNullOrEmpty(criteria.Group) ? $" AND AssociationType = @group" : string.Empty) +
                @"), Category_CTE AS
                (
	                SELECT AssociatedCategoryId Id
	                FROM Association_CTE
	                WHERE AssociatedCategoryId IS NOT NULL
	                UNION ALL
	                SELECT c.Id
	                FROM Category c
	                INNER JOIN Category_CTE cte ON c.ParentCategoryId = cte.Id
                ),
                Item_CTE AS 
                (
	                SELECT  i.Id
	                FROM (SELECT DISTINCT Id FROM Category_CTE) c
	                LEFT JOIN Item i ON c.Id=i.CategoryId WHERE i.ParentId IS NULL
	                UNION
	                SELECT AssociatedItemId Id FROM Association_CTE
                ) 
                SELECT COUNT(Id) FROM Item_CTE";

            var querySqlCommandText = @"
                    ;WITH Association_CTE AS
                    (
	                    SELECT
 		                    Id	
		                    ,AssociationType
		                    ,Priority
		                    ,ItemId
		                    ,CreatedDate
		                    ,ModifiedDate
		                    ,CreatedBy
		                    ,ModifiedBy
		                    ,Discriminator
		                    ,AssociatedItemId
		                    ,AssociatedCategoryId
		                    ,Tags
		                    ,Quantity
	                    FROM Association
	                    WHERE ItemId IN({0})"
                    + (!string.IsNullOrEmpty(criteria.Group) ? $" AND AssociationType = @group" : string.Empty) +
                    @"), Category_CTE AS
                    (
	                    SELECT AssociatedCategoryId Id, AssociatedCategoryId
	                    FROM Association_CTE
	                    WHERE AssociatedCategoryId IS NOT NULL
	                    UNION ALL
	                    SELECT c.Id, cte.AssociatedCategoryId
	                    FROM Category c
	                    INNER JOIN Category_CTE cte ON c.ParentCategoryId = cte.Id
                    ),
                    Item_CTE AS 
                    (
	                    SELECT 
		                    a.Id	
		                    ,a.AssociationType
		                    ,a.Priority
		                    ,a.ItemId
		                    ,a.CreatedDate
		                    ,a.ModifiedDate
		                    ,a.CreatedBy
		                    ,a.ModifiedBy
		                    ,a.Discriminator
		                    ,i.Id AssociatedItemId
		                    ,a.AssociatedCategoryId
		                    ,a.Tags
		                    ,a.Quantity
	                    FROM Category_CTE cat
	                    LEFT JOIN Item i ON cat.Id=i.CategoryId
	                    LEFT JOIN Association a ON cat.AssociatedCategoryId=a.AssociatedCategoryId
                        WHERE i.ParentId IS NULL
	                    UNION
	                    SELECT * FROM Association_CTE
                    ) 
                    SELECT  * FROM Item_CTE WHERE AssociatedItemId IS NOT NULL ORDER BY Priority " +
                    $"OFFSET {criteria.Skip} ROWS FETCH NEXT {criteria.Take} ROWS ONLY";

            var countSqlCommand = CreateCommand(countSqlCommandText, criteria.ObjectIds);
            var querySqlCommand = CreateCommand(querySqlCommandText, criteria.ObjectIds);
            if (!string.IsNullOrEmpty(criteria.Group))
            {
                countSqlCommand.Parameters = countSqlCommand.Parameters.Concat(new[] { new SqlParameter($"@group", criteria.Group) }).ToArray();
                querySqlCommand.Parameters = querySqlCommand.Parameters.Concat(new[] { new SqlParameter($"@group", criteria.Group) }).ToArray();
            }

            result.TotalCount = ObjectContext.ExecuteStoreQuery<int>(countSqlCommand.Text, countSqlCommand.Parameters).FirstOrDefault();
            result.Results = ObjectContext.ExecuteStoreQuery<AssociationEntity>(querySqlCommand.Text, querySqlCommand.Parameters).ToList();

            return result;
        }

        public PropertyDictionaryItemEntity[] GetPropertyDictionaryItemsByIds(string[] dictItemIds)
        {
            var result = Array.Empty<PropertyDictionaryItemEntity>();

            if (!dictItemIds.IsNullOrEmpty())
            {
                result = PropertyDictionaryItems.Include(x => x.DictionaryItemValues).Where(x => dictItemIds.Contains(x.Id)).ToArray();
            }

            return result;
        }
        #endregion

        protected virtual void AddBatchDeletedEntities<T>(IList<string> ids)
            where T : Entity, new()
        {
            var entities = ids.Select(id => new T { Id = id }).ToArray<Entity>();
            AddBatchDeletedEntities(entities);
        }

        protected virtual ObjectResult<TElement> ExecuteStoreQuery<TElement>(string commandTemplate, IEnumerable<string> parameterValues)
        {
            var command = CreateCommand(commandTemplate, parameterValues);
            return ObjectContext.ExecuteStoreQuery<TElement>(command.Text, command.Parameters);
        }

        protected virtual void ExecuteStoreCommand(string commandTemplate, IEnumerable<string> parameterValues)
        {
            var command = CreateCommand(commandTemplate, parameterValues);
            ObjectContext.ExecuteStoreCommand(command.Text, command.Parameters);
        }

        protected virtual Command CreateCommand(string commandTemplate, IEnumerable<string> parameterValues)
        {
            var parameters = parameterValues.Select((v, i) => new SqlParameter($"@p{i}", v)).ToArray();
            var parameterNames = string.Join(",", parameters.Select(p => p.ParameterName));

            return new Command
            {
                Text = string.Format(commandTemplate, parameterNames),
                Parameters = parameters.OfType<object>().ToArray(),
            };
        }

        protected class Command
        {
            public string Text { get; set; }
            public object[] Parameters { get; set; }
        }
    }
}
