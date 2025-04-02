using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public IQueryable<MeasureEntity> Measures => DbContext.Set<MeasureEntity>();
        public IQueryable<MeasureUnitEntity> MeasureUnits => DbContext.Set<MeasureUnitEntity>();
        public IQueryable<ProductConfigurationEntity> ProductConfigurations => DbContext.Set<ProductConfigurationEntity>();
        public IQueryable<ProductConfigurationSectionEntity> ProductConfigurationSections => DbContext.Set<ProductConfigurationSectionEntity>();
        public IQueryable<ProductConfigurationOptionEntity> ProductConfigurationOptions => DbContext.Set<ProductConfigurationOptionEntity>();


        public virtual async Task<IList<ProductConfigurationEntity>> GetConfigurationsByIdsAsync(IList<string> ids, CancellationToken cancellationToken)
        {
            var configurations = await ProductConfigurations
                .Where(x => ids.Contains(x.Id))
                .ToListAsync(cancellationToken);

            if (configurations.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var configurationIds = configurations.Select(x => x.Id).ToList();

                var sections = await ProductConfigurationSections
                    .Where(x => configurationIds.Contains(x.ConfigurationId))
                    .ToListAsync(cancellationToken);

                if (sections.Count > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var sectionIds = sections.Select(x => x.Id).ToList();

                    var options = await ProductConfigurationOptions
                        .Where(x => sectionIds.Contains(x.SectionId))
                        .ToListAsync(cancellationToken);

                    if (options.Count > 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var productIds = options.Select(x => x.ProductId).ToList();
                        await GetItemByIdsAsync(productIds, ItemResponseGroup.ItemInfo.ToString());
                    }
                }
            }

            return configurations;
        }

        public virtual async Task<IList<CatalogEntity>> GetCatalogsByIdsAsync(IList<string> catalogIds)
        {
            if (catalogIds.IsNullOrEmpty())
            {
                return Array.Empty<CatalogEntity>();
            }

            var result = await Catalogs
                .Include(x => x.CatalogLanguages)
                .Include(x => x.IncomingLinks)
                .Include(x => x.SeoInfos)
                .Where(x => catalogIds.Contains(x.Id))
                .AsSplitQuery()
                .ToListAsync();

            if (result.Any())
            {
                //https://docs.microsoft.com/en-us/ef/core/querying/async

                if (catalogIds.Count == 1)
                {
                    var catalogId = catalogIds.First();
                    await PropertyValues
                        .Include(x => x.DictionaryItem.DictionaryItemValues)
                        .Where(x => x.CatalogId == catalogId && x.CategoryId == null)
                        .AsSplitQuery()
                        .LoadAsync();
                }
                else
                {
                    await PropertyValues
                        .Include(x => x.DictionaryItem.DictionaryItemValues)
                        .Where(x => catalogIds.Contains(x.CatalogId) && x.CategoryId == null)
                        .AsSplitQuery()
                        .LoadAsync();
                }

                await SeoInfos.Where(x => catalogIds.Contains(x.CatalogId)).LoadAsync();

                var catalogPropertiesIds = await Properties
                    .Where(x => catalogIds.Contains(x.CatalogId) && x.CategoryId == null)
                    .Select(x => x.Id)
                    .ToListAsync();

                await GetPropertiesByIdsAsync(catalogPropertiesIds);
            }

            return result;
        }

        public virtual async Task<IList<CategoryEntity>> GetCategoriesByIdsAsync(IList<string> categoriesIds, string responseGroup)
        {
            if (categoriesIds.IsNullOrEmpty())
            {
                return Array.Empty<CategoryEntity>();
            }

            var result = await Categories
                .Include(x => x.LocalizedNames)
                .Where(x => categoriesIds.Contains(x.Id))
                .AsSplitQuery()
                .ToListAsync();

            if (result.Any())
            {
                var categoryResponseGroup = EnumUtility.SafeParseFlags(responseGroup, CategoryResponseGroup.Full);

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
                    if (categoriesIds.Count == 1)
                    {
                        // Improve CPU
                        var categoryId = categoriesIds.First();
                        await PropertyValues
                            .Include(x => x.DictionaryItem.DictionaryItemValues)
                            .Where(x => x.CategoryId == categoryId)
                            .AsSplitQuery()
                            .LoadAsync();
                    }
                    else
                    {
                        //Load category property values by separate query
                        await PropertyValues
                            .Include(x => x.DictionaryItem.DictionaryItemValues)
                            .Where(x => categoriesIds.Contains(x.CategoryId))
                            .AsSplitQuery()
                            .LoadAsync();
                    }

                    var categoryPropertiesIds = await Properties
                        .Where(x => categoriesIds.Contains(x.CategoryId))
                        .Select(x => x.Id)
                        .ToListAsync();

                    await GetPropertiesByIdsAsync(categoryPropertiesIds);
                }
            }

            return result;
        }

        public virtual async Task<IList<ItemEntity>> GetItemByIdsAsync(IList<string> itemIds, string responseGroup = null)
        {
            if (itemIds.IsNullOrEmpty())
            {
                return Array.Empty<ItemEntity>();
            }

            // Use breaking query EF performance concept https://docs.microsoft.com/en-us/ef/ef6/fundamentals/performance/perf-whitepaper#8-loading-related-entities
            var result = await Items
                .Include(x => x.LocalizedNames)
                .Include(x => x.Images)
                .Where(x => itemIds.Contains(x.Id))
                .AsSplitQuery()
                .ToListAsync();

            if (result.Any())
            {
                var itemResponseGroup = EnumUtility.SafeParseFlags(responseGroup, ItemResponseGroup.ItemLarge);

                if (itemResponseGroup.HasFlag(ItemResponseGroup.Outlines))
                {
                    itemResponseGroup |= ItemResponseGroup.Links;
                }

                if (itemResponseGroup.HasFlag(ItemResponseGroup.ItemProperties))
                {
                    if (itemIds.Count == 1)
                    {
                        var itemId = itemIds.First();
                        await PropertyValues
                            .Include(x => x.DictionaryItem.DictionaryItemValues)
                            .Where(x => x.ItemId == itemId)
                            .AsSplitQuery()
                            .LoadAsync();
                    }
                    else
                    {
                        await PropertyValues
                            .Include(x => x.DictionaryItem.DictionaryItemValues)
                            .Where(x => itemIds.Contains(x.ItemId))
                            .AsSplitQuery()
                            .LoadAsync();
                    }
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
                    IQueryable<ItemEntity> variationsQuery = Items
                        .Where(x => itemIds.Contains(x.ParentId))
                        .Include(x => x.Images)
                        .Include(x => x.ItemPropertyValues)
                        .ThenInclude(x => x.DictionaryItem.DictionaryItemValues);

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

                    await variationsQuery.AsSplitQuery().LoadAsync();
                }

                if (itemResponseGroup.HasFlag(ItemResponseGroup.ItemAssociations))
                {
                    var associations = await Associations
                        .Where(x => itemIds.Contains(x.ItemId))
                        .ToListAsync();

                    var associatedProductIds = associations
                        .Where(x => x.AssociatedItemId != null)
                        .Select(x => x.AssociatedItemId)
                        .Distinct()
                        .ToList();

                    var associatedCategoryIds = associations
                        .Where(x => x.AssociatedCategoryId != null)
                        .Select(x => x.AssociatedCategoryId)
                        .Distinct()
                        .ToList();

                    await GetItemByIdsAsync(associatedProductIds, (ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemAssets).ToString());
                    await GetCategoriesByIdsAsync(associatedCategoryIds, (CategoryResponseGroup.Info | CategoryResponseGroup.WithImages).ToString());
                }

                if (itemResponseGroup.HasFlag(ItemResponseGroup.ReferencedAssociations))
                {
                    var referencedAssociations = await Associations
                        .Where(x => itemIds.Contains(x.AssociatedItemId))
                        .ToListAsync();

                    var referencedProductIds = referencedAssociations
                        .Select(x => x.ItemId)
                        .Distinct()
                        .ToList();

                    await GetItemByIdsAsync(referencedProductIds, ItemResponseGroup.ItemInfo.ToString());
                }

                // Load parents
                var parentIds = result
                    .Where(x => x.Parent == null && x.ParentId != null)
                    .Select(x => x.ParentId)
                    .ToList();

                await GetItemByIdsAsync(parentIds, responseGroup);
            }

            return result;
        }

        public virtual async Task<IList<PropertyEntity>> GetPropertiesByIdsAsync(IList<string> propIds, bool loadDictValues = false)
        {
            if (propIds.IsNullOrEmpty())
            {
                return Array.Empty<PropertyEntity>();
            }

            //Used breaking query EF performance concept https://msdn.microsoft.com/en-us/data/hh949853.aspx#8
            var result = await Properties
                .Where(x => propIds.Contains(x.Id))
                .Include(x => x.PropertyAttributes)
                .Include(x => x.DisplayNames)
                .Include(x => x.ValidationRules)
                .AsSplitQuery()
                .ToListAsync();

            if (result.Any() && loadDictValues)
            {
                await PropertyDictionaryItems
                    .Include(x => x.DictionaryItemValues)
                    .Where(x => propIds.Contains(x.PropertyId))
                    .AsSplitQuery()
                    .LoadAsync();
            }

            return result;
        }

        /// <summary>
        /// Returned all properties belongs to specified catalog
        /// For virtual catalog also include all properties for categories linked to this virtual catalog
        /// </summary>
        /// <param name="catalogId"></param>
        /// <returns></returns>
        public virtual async Task<IList<PropertyEntity>> GetAllCatalogPropertiesAsync(string catalogId)
        {
            if (string.IsNullOrEmpty(catalogId))
            {
                return Array.Empty<PropertyEntity>();
            }

            var catalog = await Catalogs.FirstOrDefaultAsync(x => x.Id == catalogId);
            if (catalog == null)
            {
                return Array.Empty<PropertyEntity>();
            }

            if (!catalog.Virtual)
            {
                var propertyIds = await Properties
                    .Where(x => x.CatalogId == catalogId)
                    .Select(x => x.Id)
                    .ToListAsync();

                return await GetPropertiesByIdsAsync(propertyIds);
            }

            // Get all category relations
            var linkedCategoryIds = await CategoryLinks
                .Where(x => x.TargetCatalogId == catalogId)
                .Select(x => x.SourceCategoryId)
                .Distinct()
                .ToListAsync();

            // linked product categories
            var linkedProductCategoryIds = await CategoryItemRelations
                .Where(x => x.CatalogId == catalogId)
                .Join(Items, link => link.ItemId, item => item.Id, (link, item) => item)
                .Select(x => x.CategoryId)
                .Distinct()
                .ToListAsync();

            // linked products catalogs for products without category
            var categorylessItemsCatalogIds = await CategoryItemRelations
                .Where(x => x.CatalogId == catalogId)
                .Join(Items, link => link.ItemId, item => item.Id, (link, item) => item)
                .Where(x => x.CategoryId == null)
                .Select(x => x.CatalogId)
                .Distinct()
                .ToListAsync();

            linkedCategoryIds = linkedCategoryIds.Concat(linkedProductCategoryIds)
                .Distinct()
                .Where(x => x != null)
                .ToList();

            var expandedFlatLinkedCategoryIds = linkedCategoryIds.Concat(await GetAllChildrenCategoriesIdsAsync(linkedCategoryIds))
                .Distinct()
                .ToList();

            var linkedCatalogIds = await Categories
                .Where(x => expandedFlatLinkedCategoryIds.Contains(x.Id))
                .Select(x => x.CatalogId)
                .Distinct()
                .ToListAsync();

            linkedCatalogIds = linkedCatalogIds.Concat(categorylessItemsCatalogIds)
                .Distinct()
                .ToList();

            var result = await Properties
                .Where(x => linkedCatalogIds.Contains(x.CatalogId))
                .Include(x => x.PropertyAttributes)
                .Include(x => x.DisplayNames)
                .Include(x => x.ValidationRules)
                .AsSplitQuery()
                .ToListAsync();

            return result;
        }

        public virtual async Task<IList<PropertyDictionaryItemEntity>> GetPropertyDictionaryItemsByIdsAsync(IList<string> dictItemIds)
        {
            if (dictItemIds.IsNullOrEmpty())
            {
                return Array.Empty<PropertyDictionaryItemEntity>();
            }

            return await PropertyDictionaryItems
                .Include(x => x.DictionaryItemValues)
                .Where(x => dictItemIds.Contains(x.Id))
                .AsSplitQuery()
                .ToListAsync();
        }

        public virtual async Task<IList<AssociationEntity>> GetAssociationsByIdsAsync(IList<string> associationIds)
        {
            if (associationIds.IsNullOrEmpty())
            {
                return Array.Empty<AssociationEntity>();
            }

            return await Associations
                .Where(x => associationIds.Contains(x.Id))
                .ToListAsync();
        }

        public virtual Task<IList<string>> GetAllSeoDuplicatesIdsAsync()
        {
            return _rawDatabaseCommand.GetAllSeoDuplicatesIdsAsync(DbContext);
        }

        public virtual Task<IList<string>> GetAllChildrenCategoriesIdsAsync(IList<string> categoryIds)
        {
            return _rawDatabaseCommand.GetAllChildrenCategoriesIdsAsync(DbContext, categoryIds);
        }

        public virtual async Task RemoveItemsAsync(IList<string> itemIds)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _rawDatabaseCommand.RemoveItemsAsync(DbContext, itemIds);

                scope.Complete();
            }
        }

        public virtual async Task RemoveCategoriesAsync(IList<string> ids)
        {
            if (ids.IsNullOrEmpty())
            {
                return;
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var allCategoryIds = (await GetAllChildrenCategoriesIdsAsync(ids)).Concat(ids).ToList();

                var itemIds = await Items
                    .Where(i => allCategoryIds.Contains(i.CategoryId))
                    .Select(i => i.Id)
                    .ToListAsync();

                await RemoveItemsAsync(itemIds);

                await _rawDatabaseCommand.RemoveCategoriesAsync(DbContext, allCategoryIds);

                scope.Complete();
            }
        }

        public virtual async Task RemoveCatalogsAsync(IList<string> ids)
        {
            if (ids.IsNullOrEmpty())
            {
                return;
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var itemIds = await Items
                    .Where(i => ids.Contains(i.CatalogId))
                    .Select(i => i.Id)
                    .ToListAsync();

                await RemoveItemsAsync(itemIds);

                var categoryIds = await Categories
                    .Where(c => ids.Contains(c.CatalogId))
                    .Select(c => c.Id)
                    .ToListAsync();

                await RemoveCategoriesAsync(categoryIds);

                await _rawDatabaseCommand.RemoveCatalogsAsync(DbContext, ids);

                scope.Complete();
            }
        }

        /// <summary>
        /// Delete all existing property values that belong to given property.
        /// Because PropertyValue table doesn't have a foreign key to Property table by design,
        /// we use columns Name and TargetType to find values that reference the deleting property.
        /// </summary>
        /// <param name="propertyId"></param>
        public virtual async Task RemoveAllPropertyValuesAsync(string propertyId)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var properties = await GetPropertiesByIdsAsync(new[] { propertyId });

                var catalogProperty = properties.FirstOrDefault(x => x.TargetType.EqualsInvariant(PropertyType.Catalog.ToString()));
                var categoryProperty = properties.FirstOrDefault(x => x.TargetType.EqualsInvariant(PropertyType.Category.ToString()));

                var itemProperty = properties.FirstOrDefault(x =>
                    x.TargetType.EqualsInvariant(PropertyType.Product.ToString()) ||
                    x.TargetType.EqualsInvariant(PropertyType.Variation.ToString()));

                await _rawDatabaseCommand.RemoveAllPropertyValuesAsync(DbContext, catalogProperty, categoryProperty, itemProperty);

                scope.Complete();
            }
        }

        /// <summary>
        /// Searches associations by specific search criteria. 
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
        public virtual async Task<IList<CategoryEntity>> SearchCategoriesHierarchyAsync(string categoryId)
        {
            var result = await _rawDatabaseCommand.SearchCategoriesHierarchyAsync(DbContext, categoryId);

            if (result.Any())
            {
                await Images
                    .Where(x => x.CategoryId == categoryId)
                    .LoadAsync();

                await PropertyValues
                    .Include(x => x.DictionaryItem.DictionaryItemValues)
                    .Where(x => x.CategoryId == categoryId)
                    .AsSplitQuery()
                    .LoadAsync();

                await CategoryDescriptions
                    .Where(x => x.CategoryId == categoryId)
                    .LoadAsync();

                var categoryIds = result.Select(x => x.Id).ToList();

                await CategoryLinks
                    .Where(x => categoryIds.Contains(x.TargetCategoryId) || categoryIds.Contains(x.SourceCategoryId))
                    .LoadAsync();

                var categoryPropertiesIds = await Properties
                    .Where(x => categoryIds.Contains(x.CategoryId))
                    .Select(x => x.Id)
                    .ToListAsync();

                await GetPropertiesByIdsAsync(categoryPropertiesIds);

                //get all possible unique category seo infos
                await SeoInfos
                    .Where(x => categoryIds.Contains(x.CategoryId))
                    .LoadAsync();
            }

            return result;
        }

        public async Task<IList<MeasureEntity>> GetMeasuresByIdsAsync(IList<string> ids)
        {
            var measures = await Measures
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            if (measures.Count > 0)
            {
                var existingIds = measures.Select(x => x.Id).ToList();

                await MeasureUnits
                    .Include(x => x.LocalizedNames)
                    .Include(x => x.LocalizedSymbols)
                    .Where(x => existingIds.Contains(x.MeasureId))
                    .AsSplitQuery()
                    .LoadAsync();
            }

            return measures;
        }
    }
}
