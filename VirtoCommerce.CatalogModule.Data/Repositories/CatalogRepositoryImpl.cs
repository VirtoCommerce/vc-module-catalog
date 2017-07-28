using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using coreModel = VirtoCommerce.Domain.Catalog.Model;
using dataModel = VirtoCommerce.CatalogModule.Data.Model;

namespace VirtoCommerce.CatalogModule.Data.Repositories
{
    public class CatalogRepositoryImpl : EFRepositoryBase, ICatalogRepository
    {
        private readonly Func<ISearchProvider> _searchProviderFactory;
        private ISearchProvider _searchProvider;
        private ISearchProvider SearchProvider => _searchProvider ?? (_searchProvider = _searchProviderFactory());

        public CatalogRepositoryImpl()
        {
        }

        public CatalogRepositoryImpl(Func<ISearchProvider> searchProviderFactory, string nameOrConnectionString, params IInterceptor[] interceptors)
            : base(nameOrConnectionString, null, interceptors)
        {
            _searchProviderFactory = searchProviderFactory;
            Database.SetInitializer<CatalogRepositoryImpl>(null);
        }

        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();


            #region Catalog
            modelBuilder.Entity<dataModel.CatalogEntity>().ToTable("Catalog").HasKey(x => x.Id).Property(x => x.Id);
            #endregion

            #region Category
            modelBuilder.Entity<dataModel.CategoryEntity>().ToTable("Category").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<dataModel.CategoryEntity>().HasOptional(x => x.ParentCategory).WithMany().HasForeignKey(x => x.ParentCategoryId).WillCascadeOnDelete(false);
            modelBuilder.Entity<dataModel.CategoryEntity>().HasRequired(x => x.Catalog).WithMany().HasForeignKey(x => x.CatalogId).WillCascadeOnDelete(true);
            #endregion

            #region Item
            modelBuilder.Entity<dataModel.ItemEntity>().ToTable("Item").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<dataModel.ItemEntity>().HasRequired(m => m.Catalog).WithMany().HasForeignKey(x => x.CatalogId).WillCascadeOnDelete(false);
            modelBuilder.Entity<dataModel.ItemEntity>().HasOptional(m => m.Category).WithMany().HasForeignKey(x => x.CategoryId).WillCascadeOnDelete(false);
            modelBuilder.Entity<dataModel.ItemEntity>().HasOptional(m => m.Parent).WithMany(x => x.Childrens).HasForeignKey(x => x.ParentId).WillCascadeOnDelete(false);
            #endregion

            #region Property
            modelBuilder.Entity<dataModel.PropertyEntity>().ToTable("Property").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<dataModel.PropertyEntity>().HasOptional(m => m.Catalog).WithMany(x => x.Properties).HasForeignKey(x => x.CatalogId).WillCascadeOnDelete(false);
            modelBuilder.Entity<dataModel.PropertyEntity>().HasOptional(m => m.Category).WithMany(x => x.Properties).HasForeignKey(x => x.CategoryId).WillCascadeOnDelete(false);

            #endregion

            #region PropertyDictionaryValue
            modelBuilder.Entity<dataModel.PropertyDictionaryValueEntity>().ToTable("PropertyDictionaryValue").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<dataModel.PropertyDictionaryValueEntity>().HasRequired(m => m.Property).WithMany(x => x.DictionaryValues).HasForeignKey(x => x.PropertyId).WillCascadeOnDelete(true);
            #endregion

            #region PropertyAttribute
            modelBuilder.Entity<dataModel.PropertyAttributeEntity>().ToTable("PropertyAttribute").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<dataModel.PropertyAttributeEntity>().HasRequired(m => m.Property).WithMany(x => x.PropertyAttributes).HasForeignKey(x => x.PropertyId).WillCascadeOnDelete(true);
            #endregion

            #region PropertyDisplayName
            modelBuilder.Entity<dataModel.PropertyDisplayNameEntity>().ToTable("PropertyDisplayName").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<dataModel.PropertyDisplayNameEntity>().HasRequired(m => m.Property).WithMany(x => x.DisplayNames).HasForeignKey(x => x.PropertyId).WillCascadeOnDelete(true);
            #endregion


            #region PropertyValue
            modelBuilder.Entity<dataModel.PropertyValueEntity>().ToTable("PropertyValue").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<dataModel.PropertyValueEntity>().HasOptional(m => m.CatalogItem).WithMany(x => x.ItemPropertyValues).HasForeignKey(x => x.ItemId).WillCascadeOnDelete(false);
            modelBuilder.Entity<dataModel.PropertyValueEntity>().HasOptional(m => m.Category).WithMany(x => x.CategoryPropertyValues).HasForeignKey(x => x.CategoryId).WillCascadeOnDelete(false);
            modelBuilder.Entity<dataModel.PropertyValueEntity>().HasOptional(m => m.Catalog).WithMany(x => x.CatalogPropertyValues).HasForeignKey(x => x.CatalogId).WillCascadeOnDelete(false);
            #endregion

            #region PropertyValidationRule
            modelBuilder.Entity<dataModel.PropertyValidationRuleEntity>().ToTable("PropertyValidationRule").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<dataModel.PropertyValidationRuleEntity>().HasRequired(m => m.Property).WithMany(x => x.ValidationRules).HasForeignKey(x => x.PropertyId).WillCascadeOnDelete(true);

            #endregion

            #region CatalogImage
            modelBuilder.Entity<dataModel.ImageEntity>().ToTable("CatalogImage").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<dataModel.ImageEntity>().HasOptional(m => m.Category).WithMany(x => x.Images).HasForeignKey(x => x.CategoryId).WillCascadeOnDelete(false);
            modelBuilder.Entity<dataModel.ImageEntity>().HasOptional(m => m.CatalogItem).WithMany(x => x.Images).HasForeignKey(x => x.ItemId).WillCascadeOnDelete(false);
            #endregion

            #region EditorialReview
            modelBuilder.Entity<dataModel.EditorialReviewEntity>().ToTable("EditorialReview").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<dataModel.EditorialReviewEntity>().HasRequired(x => x.CatalogItem).WithMany(x => x.EditorialReviews).HasForeignKey(x => x.ItemId).WillCascadeOnDelete(true);
            #endregion         

            #region Association
            modelBuilder.Entity<dataModel.AssociationEntity>().ToTable("Association").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<dataModel.AssociationEntity>().HasRequired(m => m.Item).WithMany(x => x.Associations).HasForeignKey(x => x.ItemId).WillCascadeOnDelete(false);
            modelBuilder.Entity<dataModel.AssociationEntity>().HasOptional(m => m.AssociatedItem).WithMany().HasForeignKey(x => x.AssociatedItemId).WillCascadeOnDelete(false);
            modelBuilder.Entity<dataModel.AssociationEntity>().HasOptional(m => m.AssociatedCategory).WithMany().HasForeignKey(x => x.AssociatedCategoryId).WillCascadeOnDelete(false);
            #endregion

            #region Asset
            modelBuilder.Entity<dataModel.AssetEntity>().ToTable("CatalogAsset").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<dataModel.AssetEntity>().HasRequired(m => m.CatalogItem).WithMany(x => x.Assets).HasForeignKey(x => x.ItemId).WillCascadeOnDelete(true);
            #endregion

            #region CatalogLanguage
            modelBuilder.Entity<dataModel.CatalogLanguageEntity>().ToTable("CatalogLanguage").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<dataModel.CatalogLanguageEntity>().HasRequired(m => m.Catalog).WithMany(x => x.CatalogLanguages).HasForeignKey(x => x.CatalogId).WillCascadeOnDelete(true);
            #endregion



            #region CategoryItemRelation
            modelBuilder.Entity<dataModel.CategoryItemRelationEntity>().ToTable("CategoryItemRelation").HasKey(x => x.Id).Property(x => x.Id);

            modelBuilder.Entity<dataModel.CategoryItemRelationEntity>().HasOptional(p => p.Category).WithMany().HasForeignKey(x => x.CategoryId).WillCascadeOnDelete(false);
            modelBuilder.Entity<dataModel.CategoryItemRelationEntity>().HasRequired(p => p.CatalogItem).WithMany(x => x.CategoryLinks).HasForeignKey(x => x.ItemId).WillCascadeOnDelete(false);
            modelBuilder.Entity<dataModel.CategoryItemRelationEntity>().HasRequired(p => p.Catalog).WithMany().HasForeignKey(x => x.CatalogId).WillCascadeOnDelete(false);
            #endregion


            #region CategoryRelation
            modelBuilder.Entity<dataModel.CategoryRelationEntity>().ToTable("CategoryRelation").HasKey(x => x.Id).Property(x => x.Id);

            modelBuilder.Entity<dataModel.CategoryRelationEntity>().HasOptional(x => x.TargetCategory)
                                       .WithMany(x => x.IncommingLinks)
                                       .HasForeignKey(x => x.TargetCategoryId).WillCascadeOnDelete(false);

            modelBuilder.Entity<dataModel.CategoryRelationEntity>().HasRequired(x => x.SourceCategory)
                                       .WithMany(x => x.OutgoingLinks)
                                       .HasForeignKey(x => x.SourceCategoryId).WillCascadeOnDelete(false);

            modelBuilder.Entity<dataModel.CategoryRelationEntity>().HasOptional(x => x.TargetCatalog)
                                       .WithMany(x => x.IncommingLinks)
                                       .HasForeignKey(x => x.TargetCatalogId).WillCascadeOnDelete(false);
            #endregion
            base.OnModelCreating(modelBuilder);
        }

        #region ICatalogRepository Members
        public IQueryable<dataModel.CategoryEntity> Categories
        {
            get { return GetAsQueryable<dataModel.CategoryEntity>(); }
        }

        public IQueryable<dataModel.CatalogEntity> Catalogs
        {
            get { return GetAsQueryable<dataModel.CatalogEntity>(); }
        }

        public IQueryable<dataModel.PropertyValueEntity> PropertyValues
        {
            get { return GetAsQueryable<dataModel.PropertyValueEntity>(); }
        }

        public IQueryable<dataModel.ImageEntity> Images
        {
            get { return GetAsQueryable<dataModel.ImageEntity>(); }
        }
        public IQueryable<dataModel.AssetEntity> Assets
        {
            get { return GetAsQueryable<dataModel.AssetEntity>(); }
        }

        public IQueryable<dataModel.ItemEntity> Items
        {
            get { return GetAsQueryable<dataModel.ItemEntity>(); }
        }

        public IQueryable<dataModel.EditorialReviewEntity> EditorialReviews
        {
            get { return GetAsQueryable<dataModel.EditorialReviewEntity>(); }
        }

        public IQueryable<dataModel.PropertyEntity> Properties
        {
            get { return GetAsQueryable<dataModel.PropertyEntity>(); }
        }

        public IQueryable<dataModel.PropertyDictionaryValueEntity> PropertyDictionaryValues
        {
            get { return GetAsQueryable<dataModel.PropertyDictionaryValueEntity>(); }
        }

        public IQueryable<dataModel.PropertyDisplayNameEntity> PropertyDisplayNames
        {
            get { return GetAsQueryable<dataModel.PropertyDisplayNameEntity>(); }
        }
        public IQueryable<dataModel.PropertyAttributeEntity> PropertyAttributes
        {
            get { return GetAsQueryable<dataModel.PropertyAttributeEntity>(); }
        }

        public IQueryable<dataModel.CategoryItemRelationEntity> CategoryItemRelations
        {
            get { return GetAsQueryable<dataModel.CategoryItemRelationEntity>(); }
        }

        public IQueryable<dataModel.AssociationEntity> Associations
        {
            get { return GetAsQueryable<dataModel.AssociationEntity>(); }
        }

        public IQueryable<dataModel.CategoryRelationEntity> CategoryLinks
        {
            get { return GetAsQueryable<dataModel.CategoryRelationEntity>(); }
        }

        public IQueryable<dataModel.PropertyValidationRuleEntity> PropertyValidationRules
        {
            get { return GetAsQueryable<dataModel.PropertyValidationRuleEntity>(); }
        }

        public dataModel.CatalogEntity[] GetCatalogsByIds(string[] catalogIds)
        {
            var retVal = Catalogs.Include(x => x.CatalogLanguages)
                                 .Include(x => x.IncommingLinks)
                                 .Where(x => catalogIds.Contains(x.Id))
                                 .ToArray();

            var propertyValues = PropertyValues.Where(x => catalogIds.Contains(x.CatalogId) && x.CategoryId == null).ToArray();
            var catalogPropertiesIds = Properties.Where(x => catalogIds.Contains(x.CatalogId) && x.CategoryId == null)
                                                 .Select(x => x.Id)
                                                 .ToArray();
            var catalogProperties = GetPropertiesByIds(catalogPropertiesIds);
       
            return retVal;
        }



        public string[] GetAllChildrenCategoriesIds(string[] categoryIds)
        {
            var retVal = new List<string>();
            if (!categoryIds.IsNullOrEmpty())
            {
                const string queryPattern =
                    @"WITH cte AS  ( SELECT a.Id FROM Category a  WHERE Id IN ({0})
                  UNION ALL
                  SELECT a.Id FROM Category a JOIN cte c ON a.ParentCategoryId = c.Id)
                  SELECT Id FROM cte WHERE Id NOT IN ({0})";

                var query = string.Format(queryPattern, string.Join(", ", categoryIds.Select(x => string.Format("'{0}'", x))));
                retVal = ObjectContext.ExecuteStoreQuery<string>(query).ToList();
            }
            return retVal.ToArray();
        }

        public dataModel.CategoryEntity[] GetCategoriesByIds(string[] categoriesIds, coreModel.CategoryResponseGroup respGroup)
        {
            if (categoriesIds == null)
            {
                throw new ArgumentNullException("categoriesIds");
            }

            if (!categoriesIds.Any())
            {
                return new dataModel.CategoryEntity[] { };
            }

            if (respGroup.HasFlag(coreModel.CategoryResponseGroup.WithOutlines))
            {
                respGroup |= coreModel.CategoryResponseGroup.WithLinks | coreModel.CategoryResponseGroup.WithParents;
            }

            var result = Categories.Where(x => categoriesIds.Contains(x.Id)).ToArray();

            if (respGroup.HasFlag(coreModel.CategoryResponseGroup.WithLinks))
            {
                var incommingLinks = CategoryLinks.Where(x => categoriesIds.Contains(x.TargetCategoryId)).ToArray();
                var outgoingLinks = CategoryLinks.Where(x => categoriesIds.Contains(x.SourceCategoryId)).ToArray();
            }

            if (respGroup.HasFlag(coreModel.CategoryResponseGroup.WithImages))
            {
                var images = Images.Where(x => categoriesIds.Contains(x.CategoryId)).ToArray();
            }

            //Load category property values by separate query
            var propertyValues = PropertyValues.Where(x => categoriesIds.Contains(x.CategoryId)).ToArray();

            //Load all properties meta information and information for inheritance
            if (respGroup.HasFlag(coreModel.CategoryResponseGroup.WithProperties))
            {
                var categoryPropertiesIds = Properties.Where(x => categoriesIds.Contains(x.CategoryId)).Select(x=>x.Id).ToArray();
                var categoryProperties = GetPropertiesByIds(categoryPropertiesIds);             
            }

            return result;
        }

        public dataModel.ItemEntity[] GetItemByIds(string[] itemIds, coreModel.ItemResponseGroup respGroup = coreModel.ItemResponseGroup.ItemLarge)
        {
            if (itemIds == null)
            {
                throw new ArgumentNullException("itemIds");
            }

            if (!itemIds.Any())
            {
                return new dataModel.ItemEntity[] { };
            }

            // Use breaking query EF performance concept https://msdn.microsoft.com/en-us/data/hh949853.aspx#8
            var retVal = Items.Include(x => x.Images).Where(x => itemIds.Contains(x.Id)).ToArray();
            var propertyValues = PropertyValues.Where(x => itemIds.Contains(x.ItemId)).ToArray();

            if (respGroup.HasFlag(coreModel.ItemResponseGroup.Outlines))
            {
                respGroup |= coreModel.ItemResponseGroup.Links;
            }

            if (respGroup.HasFlag(coreModel.ItemResponseGroup.Links))
            {
                var relations = CategoryItemRelations.Where(x => itemIds.Contains(x.ItemId)).ToArray();

            }

            if (respGroup.HasFlag(coreModel.ItemResponseGroup.ItemAssets))
            {
                var assets = Assets.Where(x => itemIds.Contains(x.ItemId)).ToArray();
            }

            if (respGroup.HasFlag(coreModel.ItemResponseGroup.ItemEditorialReviews))
            {
                var editorialReviews = EditorialReviews.Where(x => itemIds.Contains(x.ItemId)).ToArray();
            }

            if (respGroup.HasFlag(coreModel.ItemResponseGroup.Variations))
            {
                // TODO: Call GetItemByIds for variations recursively (need to measure performance and data amount first)

                var variationIds = Items.Where(x => itemIds.Contains(x.ParentId)).Select(x => x.Id).ToArray();

                // Always load info, images and property values for variations
                var variations = Items.Include(x => x.Images).Where(x => variationIds.Contains(x.Id)).ToArray();
                var variationPropertyValues = PropertyValues.Where(x => variationIds.Contains(x.ItemId)).ToArray();

                if (respGroup.HasFlag(coreModel.ItemResponseGroup.ItemAssets))
                {
                    var variationAssets = Assets.Where(x => variationIds.Contains(x.ItemId)).ToArray();
                }

                if (respGroup.HasFlag(coreModel.ItemResponseGroup.ItemEditorialReviews))
                {
                    var variationEditorialReviews = EditorialReviews.Where(x => variationIds.Contains(x.ItemId)).ToArray();
                }
            }

            if (respGroup.HasFlag(coreModel.ItemResponseGroup.ItemAssociations))
            {
                var assosiations = Associations.Where(x => itemIds.Contains(x.ItemId)).ToArray();
                var assosiatedProductIds = assosiations.Where(x => x.AssociatedItemId != null)
                                                       .Select(x => x.AssociatedItemId).Distinct().ToArray();

                var assosiatedItems = GetItemByIds(assosiatedProductIds, coreModel.ItemResponseGroup.ItemInfo | coreModel.ItemResponseGroup.ItemAssets);
            }

            // Load parents
            var parentIds = retVal.Where(x => x.Parent == null && x.ParentId != null).Select(x => x.ParentId).ToArray();
            var parents = GetItemByIds(parentIds, respGroup);
            return retVal;
        }

        public dataModel.PropertyEntity[] GetPropertiesByIds(string[] propIds)
        {
            //Used breaking query EF performance concept https://msdn.microsoft.com/en-us/data/hh949853.aspx#8
            var retVal = Properties.Where(x => propIds.Contains(x.Id)).ToArray();
            var propAttributes = PropertyAttributes.Where(x => propIds.Contains(x.PropertyId)).ToArray();
            var propDisplayNames = PropertyDisplayNames.Where(x => propIds.Contains(x.PropertyId)).ToArray();
            var propValidationRules = PropertyValidationRules.Where(x => propIds.Contains(x.PropertyId)).ToArray();

            //Do not load dictionary values for not enum properties
            var dictPropertiesIds = retVal.Where(x => x.IsEnum).Select(x => x.Id).ToArray();
            if (!dictPropertiesIds.IsNullOrEmpty())
            {
                var dictValues = PropertyDictionaryValues.Where(x => dictPropertiesIds.Contains(x.PropertyId)).ToArray();
            }
            return retVal;
        }

        /// <summary>
        /// Returned all properties belongs to specified catalog 
        /// For virtual catalog also include all properties for categories linked to this virtual catalog 
        /// </summary>
        /// <param name="catalogId"></param>
        /// <returns></returns>
        public dataModel.PropertyEntity[] GetAllCatalogProperties(string catalogId)
        {
            var retVal = new List<dataModel.PropertyEntity>();
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

                    propertyIds = propertyIds.Concat(Properties.Where(x => expandedFlatLinkedCategoryIds.Contains(x.CategoryId)).Select(x => x.Id)).Distinct().ToArray();
                    var linkedCatalogIds = Categories.Where(x => expandedFlatLinkedCategoryIds.Contains(x.Id)).Select(x => x.CatalogId).Distinct().ToArray();
                    propertyIds = propertyIds.Concat(Properties.Where(x => linkedCatalogIds.Contains(x.CatalogId) && x.CategoryId == null).Select(x => x.Id)).Distinct().ToArray();
                }
                retVal.AddRange(GetPropertiesByIds(propertyIds));
            }
            return retVal.ToArray();

        }

        public void RemoveItems(string[] itemIds)
        {
            if (itemIds == null || !itemIds.Any())
                return;

            const string queryPattern =
            @"
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

            DELETE  FROM Item  WHERE Id IN ({0})";

            var query = string.Format(queryPattern, string.Join(", ", itemIds.Select(x => string.Format("'{0}'", x))));

            ObjectContext.ExecuteStoreCommand(query);

            // Remove from index directly because we bypass the repository's changeTracker by executing delete statements manually.
            RemoveFromIndex(KnownDocumentTypes.Product, itemIds);
        }

        public void RemoveCategories(string[] ids)
        {
            if (ids == null || !ids.Any())
                return;

            var allCategoriesIds = GetAllChildrenCategoriesIds(ids).Concat(ids);
            const string queryPattern =
            @"DELETE FROM SeoUrlKeyword WHERE ObjectType = 'Category' AND ObjectId IN ({0})
            DELETE CI FROM CatalogImage CI INNER JOIN Category C ON C.Id = CI.CategoryId WHERE C.Id IN ({0}) 
            DELETE PV FROM PropertyValue PV INNER JOIN Category C ON C.Id = PV.CategoryId WHERE C.Id IN ({0}) 
            DELETE CR FROM CategoryRelation CR INNER JOIN Category C ON C.Id = CR.SourceCategoryId OR C.Id = CR.TargetCategoryId  WHERE C.Id IN ({0}) 
            DELETE CIR FROM CategoryItemRelation CIR INNER JOIN Category C ON C.Id = CIR.CategoryId WHERE C.Id IN ({0}) 
            DELETE A FROM Association A INNER JOIN Category C ON C.Id = A.AssociatedCategoryId WHERE C.Id IN ({0})
            DELETE P FROM Property P INNER JOIN Category C ON C.Id = P.CategoryId  WHERE C.Id IN ({0})";


            var itemsIds = Items.Where(x => allCategoriesIds.Contains(x.CategoryId)).Select(x => x.Id).ToArray();
            int skip = 0;
            var take = 500;
            do
            {
                var itemIdsPart = itemsIds.Skip(skip).Take(take).ToArray();
                if (!itemIdsPart.IsNullOrEmpty())
                {
                    RemoveItems(itemIdsPart);
                }
                skip += take;
            } while (skip <= itemsIds.Count());


            var query = string.Format(queryPattern, string.Join(", ", allCategoriesIds.Select(x => string.Format("'{0}'", x))));
            var queryBuilder = new StringBuilder(query);
            //Need remove categories in prior hierarchy order from  child to parent
            queryBuilder.AppendLine(string.Format("DELETE FROM Category WHERE Id IN ({0})", string.Join(", ", allCategoriesIds.Select(x => string.Format("'{0}'", x)))));

            ObjectContext.ExecuteStoreCommand(queryBuilder.ToString());

            // Remove from index directly because we bypass the repository's changeTracker by executing delete statements manually.
            RemoveFromIndex(KnownDocumentTypes.Category, ids);
        }

        public void RemoveCatalogs(string[] ids)
        {
            if (ids == null || !ids.Any())
                return;

            var catalogCategoriesIds = Categories.Where(x => ids.Contains(x.CatalogId)).Select(x => x.Id).ToArray();
            var catalogItemsIds = Items.Where(x => x.CategoryId == null && ids.Contains(x.CatalogId)).Select(x => x.Id).ToArray();

            RemoveItems(catalogItemsIds);
            RemoveCategories(catalogCategoriesIds);

            const string queryPattern =
            @"DELETE CL FROM CatalogLanguage CL INNER JOIN Catalog C ON C.Id = CL.CatalogId WHERE C.Id IN ({0})
            DELETE CR FROM CategoryRelation CR INNER JOIN Catalog C ON C.Id = CR.TargetCatalogId WHERE C.Id IN ({0}) 
            DELETE PV FROM PropertyValue PV INNER JOIN Catalog C ON C.Id = PV.CatalogId WHERE C.Id IN ({0}) 
            DELETE P FROM Property P INNER JOIN Catalog C ON C.Id = P.CatalogId  WHERE C.Id IN ({0})
            DELETE FROM Catalog WHERE Id IN ({0})";
            var query = string.Format(queryPattern, string.Join(", ", ids.Select(x => string.Format("'{0}'", x))));
            ObjectContext.ExecuteStoreCommand(query);
        }
        #endregion

        protected virtual void RemoveFromIndex(string documentType, string[] ids)
        {
            SearchProvider?.RemoveAsync(documentType, ids.Select(x => new IndexDocument(x)).ToList());
        }
    }
}
