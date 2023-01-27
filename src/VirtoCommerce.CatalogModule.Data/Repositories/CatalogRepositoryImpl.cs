using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Repositories
{
    public class CatalogRepositoryImpl : DbContextRepositoryBase<CatalogDbContext>, ICatalogRepository
    {
        private readonly ICatalogRawDatabaseCommand _rawDatabaseCommand;

        public CatalogRepositoryImpl(CatalogDbContext dbContext, ICatalogRawDatabaseCommand rawDatabaseCommand)
            : base(dbContext)
        {
            _rawDatabaseCommand = rawDatabaseCommand;
        }

        #region ICatalogRepository Members

        public IQueryable<CategoryEntity> Categories => DbContext.Set<CategoryEntity>();
        public IQueryable<CatalogEntity> Catalogs => DbContext.Set<CatalogEntity>();
        public IQueryable<PropertyValueEntity> PropertyValues => DbContext.Set<PropertyValueEntity>();
        public IQueryable<ImageEntity> Images => DbContext.Set<ImageEntity>();
        public IQueryable<AssetEntity> Assets => DbContext.Set<AssetEntity>();
        public IQueryable<VideoEntity> Videos => DbContext.Set<VideoEntity>();
        public IQueryable<ItemEntity> Items => DbContext.Set<ItemEntity>();
        public IQueryable<EditorialReviewEntity> EditorialReviews => DbContext.Set<EditorialReviewEntity>();
        public IQueryable<CategoryDescriptionEntity> CategoryDescriptions => DbContext.Set<CategoryDescriptionEntity>();
        public IQueryable<PropertyEntity> Properties => DbContext.Set<PropertyEntity>();
        public IQueryable<PropertyDictionaryItemEntity> PropertyDictionaryItems => DbContext.Set<PropertyDictionaryItemEntity>();
        public IQueryable<PropertyDictionaryValueEntity> PropertyDictionaryValues => DbContext.Set<PropertyDictionaryValueEntity>();
        public IQueryable<PropertyDisplayNameEntity> PropertyDisplayNames => DbContext.Set<PropertyDisplayNameEntity>();
        public IQueryable<PropertyAttributeEntity> PropertyAttributes => DbContext.Set<PropertyAttributeEntity>();
        public IQueryable<CategoryItemRelationEntity> CategoryItemRelations => DbContext.Set<CategoryItemRelationEntity>();
        public IQueryable<AssociationEntity> Associations => DbContext.Set<AssociationEntity>();
        public IQueryable<CategoryRelationEntity> CategoryLinks => DbContext.Set<CategoryRelationEntity>();
        public IQueryable<PropertyValidationRuleEntity> PropertyValidationRules => DbContext.Set<PropertyValidationRuleEntity>();
        public IQueryable<SeoInfoEntity> SeoInfos => DbContext.Set<SeoInfoEntity>();

        public virtual async Task<CatalogEntity[]> GetCatalogsByIdsAsync(string[] catalogIds)
        {
            var result = Array.Empty<CatalogEntity>();

            if (!catalogIds.IsNullOrEmpty())
            {
                result = await Catalogs.Include(x => x.CatalogLanguages)
                    .Include(x => x.IncomingLinks)
                    .Where(x => catalogIds.Contains(x.Id))
                    .ToArrayAsync();

                if (result.Any())
                {
                    //https://docs.microsoft.com/en-us/ef/core/querying/async

                    await PropertyValues.Include(x => x.DictionaryItem.DictionaryItemValues)
                                                           .Where(x => catalogIds.Contains(x.CatalogId) && x.CategoryId == null).LoadAsync();
                    var catalogPropertiesIds = await Properties.Where(x => catalogIds.Contains(x.CatalogId) && x.CategoryId == null)
                        .Select(x => x.Id)
                        .ToArrayAsync();
                    await GetPropertiesByIdsAsync(catalogPropertiesIds);
                }
            }
            return result;
        }

        public virtual async Task<CategoryEntity[]> GetCategoriesByIdsAsync(string[] categoriesIds, string responseGroup)
        {
            var categoryResponseGroup = EnumUtility.SafeParseFlags(responseGroup, CategoryResponseGroup.Full);
            var result = Array.Empty<CategoryEntity>();

            if (!categoriesIds.IsNullOrEmpty())
            {
                result = await Categories.Where(x => categoriesIds.Contains(x.Id)).ToArrayAsync();

                if (result.Any())
                {
                    if (categoryResponseGroup.HasFlag(CategoryResponseGroup.WithOutlines))
                    {
                        categoryResponseGroup |= CategoryResponseGroup.WithLinks | CategoryResponseGroup.WithParents;
                    }

                    if (categoryResponseGroup.HasFlag(CategoryResponseGroup.WithLinks))
                    {
                        await CategoryLinks.Where(x => categoriesIds.Contains(x.TargetCategoryId)).LoadAsync();
                        await CategoryLinks.Where(x => categoriesIds.Contains(x.SourceCategoryId)).LoadAsync();
                    }

                    if (categoryResponseGroup.HasFlag(CategoryResponseGroup.WithImages))
                    {
                        await Images.Where(x => categoriesIds.Contains(x.CategoryId)).LoadAsync();
                    }

                    if (categoryResponseGroup.HasFlag(CategoryResponseGroup.WithSeo))
                    {
                        await SeoInfos.Where(x => categoriesIds.Contains(x.CategoryId)).LoadAsync();
                    }

                    if (categoryResponseGroup.HasFlag(CategoryResponseGroup.WithDescriptions))
                    {
                        await CategoryDescriptions.Where(x => categoriesIds.Contains(x.CategoryId)).LoadAsync();
                    }

                    //Load all properties meta information and information for inheritance
                    if (categoryResponseGroup.HasFlag(CategoryResponseGroup.WithProperties))
                    {
                        //Load category property values by separate query
                        await PropertyValues.Include(x => x.DictionaryItem.DictionaryItemValues)
                            .Where(x => categoriesIds.Contains(x.CategoryId)).LoadAsync();

                        var categoryPropertiesIds = await Properties.Where(x => categoriesIds.Contains(x.CategoryId))
                                                                    .Select(x => x.Id).ToArrayAsync();
                        await GetPropertiesByIdsAsync(categoryPropertiesIds);
                    }
                }
            }

            return result;
        }

        public virtual async Task<ItemEntity[]> GetItemByIdsAsync(string[] itemIds, string responseGroup = null)
        {
            var itemResponseGroup = EnumUtility.SafeParseFlags(responseGroup, ItemResponseGroup.ItemLarge);
            var result = Array.Empty<ItemEntity>();

            if (!itemIds.IsNullOrEmpty())
            {
                // Use breaking query EF performance concept https://docs.microsoft.com/en-us/ef/ef6/fundamentals/performance/perf-whitepaper#8-loading-related-entities
                result = await Items.Include(x => x.Images).Where(x => itemIds.Contains(x.Id)).ToArrayAsync();

                if (result.Any())
                {
                    if (itemResponseGroup.HasFlag(ItemResponseGroup.Outlines))
                    {
                        itemResponseGroup |= ItemResponseGroup.Links;
                    }

                    if (itemResponseGroup.HasFlag(ItemResponseGroup.ItemProperties))
                    {
                        await PropertyValues.Include(x => x.DictionaryItem.DictionaryItemValues).Where(x => itemIds.Contains(x.ItemId)).LoadAsync();
                    }

                    if (itemResponseGroup.HasFlag(ItemResponseGroup.Links))
                    {
                        await CategoryItemRelations.Where(x => itemIds.Contains(x.ItemId)).LoadAsync();
                    }

                    if (itemResponseGroup.HasFlag(ItemResponseGroup.ItemAssets))
                    {
                        await Assets.Where(x => itemIds.Contains(x.ItemId)).LoadAsync();
                    }

                    if (itemResponseGroup.HasFlag(ItemResponseGroup.ItemEditorialReviews))
                    {
                        await EditorialReviews.Where(x => itemIds.Contains(x.ItemId)).LoadAsync();
                    }

                    if (itemResponseGroup.HasFlag(ItemResponseGroup.WithSeo))
                    {
                        await SeoInfos.Where(x => itemIds.Contains(x.ItemId)).LoadAsync();
                    }

#pragma warning disable CS0618 // Variations can be used here
                    if (itemResponseGroup.HasFlag(ItemResponseGroup.Variations))
#pragma warning restore CS0618
                    {
                        // Call GetItemByIds for variations recursively (need to measure performance and data amount first)
                        IQueryable<ItemEntity> variationsQuery = Items.Where(x => itemIds.Contains(x.ParentId))
                                                    .Include(x => x.Images)
                                                    .Include(x => x.ItemPropertyValues).ThenInclude(x => x.DictionaryItem.DictionaryItemValues);

                        if (itemResponseGroup.HasFlag(ItemResponseGroup.ItemAssets))
                        {
                            variationsQuery = variationsQuery.Include(x => x.Assets);
                        }
                        if (itemResponseGroup.HasFlag(ItemResponseGroup.ItemEditorialReviews))
                        {
                            variationsQuery = variationsQuery.Include(x => x.EditorialReviews);
                        }
                        if (itemResponseGroup.HasFlag(ItemResponseGroup.Seo))
                        {
                            variationsQuery = variationsQuery.Include(x => x.SeoInfos);
                        }
                        await variationsQuery.LoadAsync();
                    }

                    if (itemResponseGroup.HasFlag(ItemResponseGroup.ItemAssociations))
                    {
                        var associations = await Associations.Where(x => itemIds.Contains(x.ItemId)).ToArrayAsync();
                        var associatedProductIds = associations.Where(x => x.AssociatedItemId != null)
                            .Select(x => x.AssociatedItemId).Distinct().ToArray();
                        var associatedCategoryIds = associations.Where(x => x.AssociatedCategoryId != null).Select(x => x.AssociatedCategoryId).Distinct().ToArray();

                        await GetItemByIdsAsync(associatedProductIds, (ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemAssets).ToString());
                        await GetCategoriesByIdsAsync(associatedCategoryIds, (CategoryResponseGroup.Info | CategoryResponseGroup.WithImages).ToString());
                    }

                    if (itemResponseGroup.HasFlag(ItemResponseGroup.ReferencedAssociations))
                    {
                        var referencedAssociations = await Associations.Where(x => itemIds.Contains(x.AssociatedItemId)).ToArrayAsync();
                        var referencedProductIds = referencedAssociations.Select(x => x.ItemId).Distinct().ToArray();
                        await GetItemByIdsAsync(referencedProductIds, ItemResponseGroup.ItemInfo.ToString());
                    }

                    // Load parents
                    var parentIds = result.Where(x => x.Parent == null && x.ParentId != null).Select(x => x.ParentId).ToArray();
                    await GetItemByIdsAsync(parentIds, responseGroup);
                }
            }

            return result;
        }

        public virtual async Task<PropertyEntity[]> GetPropertiesByIdsAsync(string[] propIds, bool loadDictValues = false)
        {
            var result = Array.Empty<PropertyEntity>();

            if (!propIds.IsNullOrEmpty())
            {
                //Used breaking query EF performance concept https://msdn.microsoft.com/en-us/data/hh949853.aspx#8
                result = await Properties.Where(x => propIds.Contains(x.Id))
                                         .Include(x => x.PropertyAttributes)
                                         .Include(x => x.DisplayNames)
                                         .Include(x => x.ValidationRules)
                                         .ToArrayAsync();

                if (result.Any() && loadDictValues)
                {
                    await PropertyDictionaryItems.Include(x => x.DictionaryItemValues).Where(x => propIds.Contains(x.PropertyId)).LoadAsync();
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
        public virtual async Task<PropertyEntity[]> GetAllCatalogPropertiesAsync(string catalogId)
        {
            var result = Array.Empty<PropertyEntity>();

            if (!string.IsNullOrEmpty(catalogId))
            {
                var catalog = await Catalogs.FirstOrDefaultAsync(x => x.Id == catalogId);

                if (catalog != null)
                {
                    var propertyIds = await Properties.Where(x => x.CatalogId == catalogId).Select(x => x.Id).ToArrayAsync();

                    if (catalog.Virtual)
                    {
                        //get all category relations
                        var linkedCategoryIds = await CategoryLinks.Where(x => x.TargetCatalogId == catalogId)
                            .Select(x => x.SourceCategoryId)
                            .Distinct()
                            .ToArrayAsync();
                        //linked product categories links
                        var linkedProductCategoryIds = await CategoryItemRelations.Where(x => x.CatalogId == catalogId)
                            .Join(Items, link => link.ItemId, item => item.Id, (link, item) => item)
                            .Select(x => x.CategoryId)
                            .Distinct()
                            .ToArrayAsync();
                        linkedCategoryIds = linkedCategoryIds.Concat(linkedProductCategoryIds)
                            .Distinct()
                            .Where(x => x != null)
                            .ToArray();
                        var expandedFlatLinkedCategoryIds = linkedCategoryIds.Concat(await GetAllChildrenCategoriesIdsAsync(linkedCategoryIds)).Distinct().ToArray();
                        var linkedCatalogIds = await Categories.Where(x => expandedFlatLinkedCategoryIds.Contains(x.Id)).Select(x => x.CatalogId).Distinct().ToArrayAsync();
                        return await Properties.Where(x => linkedCatalogIds.Contains(x.CatalogId))
                            .Include(x => x.PropertyAttributes)
                            .Include(x => x.DisplayNames)
                            .Include(x => x.ValidationRules)
                            .ToArrayAsync();
                    }

                    result = await GetPropertiesByIdsAsync(propertyIds);
                }
            }

            return result;
        }

        public virtual async Task<PropertyDictionaryItemEntity[]> GetPropertyDictionaryItemsByIdsAsync(string[] dictItemIds)
        {
            var result = Array.Empty<PropertyDictionaryItemEntity>();

            if (!dictItemIds.IsNullOrEmpty())
            {
                result = await PropertyDictionaryItems.Include(x => x.DictionaryItemValues)
                    .Where(x => dictItemIds.Contains(x.Id))
                    .ToArrayAsync();
            }

            return result;
        }

        public virtual async Task<AssociationEntity[]> GetAssociationsByIdsAsync(string[] associationIds)
        {
            var result = Array.Empty<AssociationEntity>();

            if (!associationIds.IsNullOrEmpty())
            {
                result = await Associations.Where(x => associationIds.Contains(x.Id)).ToArrayAsync();
            }

            return result;
        }

        public virtual Task<string[]> GetAllSeoDuplicatesIdsAsync()
        {
            return _rawDatabaseCommand.GetAllSeoDuplicatesIdsAsync(DbContext);
        }

        public virtual Task<string[]> GetAllChildrenCategoriesIdsAsync(string[] categoryIds)
        {
            return _rawDatabaseCommand.GetAllChildrenCategoriesIdsAsync(DbContext, categoryIds);
        }

        public virtual async Task RemoveItemsAsync(string[] itemIds)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _rawDatabaseCommand.RemoveItemsAsync(DbContext, itemIds);

                scope.Complete();
            }
        }

        public virtual async Task RemoveCategoriesAsync(string[] ids)
        {
            if (!ids.IsNullOrEmpty())
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var allCategoryIds = (await GetAllChildrenCategoriesIdsAsync(ids)).Concat(ids).ToArray();

                    var itemIds = await Items.Where(i => allCategoryIds.Contains(i.CategoryId)).Select(i => i.Id).ToArrayAsync();
                    await RemoveItemsAsync(itemIds);

                    await _rawDatabaseCommand.RemoveCategoriesAsync(DbContext, allCategoryIds);

                    scope.Complete();
                }
            }
        }

        public virtual async Task RemoveCatalogsAsync(string[] ids)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var itemIds = await Items.Where(i => ids.Contains(i.CatalogId)).Select(i => i.Id).ToArrayAsync();
                await RemoveItemsAsync(itemIds);

                var categoryIds = await Categories.Where(c => ids.Contains(c.CatalogId)).Select(c => c.Id).ToArrayAsync();
                await RemoveCategoriesAsync(categoryIds);

                await _rawDatabaseCommand.RemoveCatalogsAsync(DbContext, ids);

                scope.Complete();
            }
        }

        /// <summary>
        /// Delete all exist property values belong to given property.
        /// Because PropertyValue table doesn't have a foreign key to Property table by design,
        /// we use columns Name and TargetType to find values that reference to the deleting property.
        /// </summary>
        /// <param name="propertyId"></param>
        public virtual async Task RemoveAllPropertyValuesAsync(string propertyId)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var properties = await GetPropertiesByIdsAsync(new[] { propertyId });

                var catalogProperty = properties.FirstOrDefault(x => x.TargetType.EqualsInvariant(PropertyType.Catalog.ToString()));
                var categoryProperty = properties.FirstOrDefault(x => x.TargetType.EqualsInvariant(PropertyType.Category.ToString()));
                var itemProperty = properties.FirstOrDefault(x => x.TargetType.EqualsInvariant(PropertyType.Product.ToString()) || x.TargetType.EqualsInvariant(PropertyType.Variation.ToString()));

                await _rawDatabaseCommand.RemoveAllPropertyValuesAsync(DbContext, catalogProperty, categoryProperty, itemProperty);

                scope.Complete();
            }
        }

        /// <summary>
        /// Searchs associations by specific search criterias. 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>Returns SearchResult total count and items.</returns>
        public virtual Task<GenericSearchResult<AssociationEntity>> SearchAssociations(ProductAssociationSearchCriteria criteria)
        {
            return _rawDatabaseCommand.SearchAssociations(DbContext, criteria);
        }

        /// <summary>
        /// Returns requested category and all its parent categories to the root (up the hierarchy tree) in a plain list.
        /// Also loads all dependencies for said categories.
        /// </summary>
        public virtual async Task<ICollection<CategoryEntity>> SearchCategoriesHierarchyAsync(string categoryId)
        {
            var result = await _rawDatabaseCommand.SearchCategoriesHierarchyAsync(DbContext, categoryId);

            if (result.Any())
            {
                await Images.Where(x => x.CategoryId == categoryId).LoadAsync();
                await PropertyValues.Include(x => x.DictionaryItem.DictionaryItemValues).Where(x => x.CategoryId == categoryId).LoadAsync();
                await CategoryDescriptions.Where(x => x.CategoryId == categoryId).LoadAsync();

                var categoriesIds = result.Select(x => x.Id).ToList();

                await CategoryLinks.Where(x => categoriesIds.Contains(x.TargetCategoryId) || categoriesIds.Contains(x.SourceCategoryId)).LoadAsync();

                var categoryPropertiesIds = await Properties.Where(x => categoriesIds.Contains(x.CategoryId)).Select(x => x.Id).ToArrayAsync();
                await GetPropertiesByIdsAsync(categoryPropertiesIds);

                //get all possible unique category seo infos
                await SeoInfos.Where(x => categoriesIds.Contains(x.CategoryId)).LoadAsync();
            }

            return result;

        }

        #endregion ICatalogRepository Members
    }
}
