using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.BulkActionsModule.Core.Services;
using VirtoCommerce.CatalogModule.BulkActions.Models;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.BulkActions.DataSources;

public class PropertyDataSource : IDataSource
{
    private readonly Func<ICatalogRepository> _repositoryFactory;
    private readonly ICategoryService _categoryService;
    private readonly IPropertyService _propertyService;
    private readonly IItemService _itemService;
    private readonly DataQuery _dataQuery;

    private bool _fetchExecuted;

    private readonly IEqualityComparer<Property> _propertyComparer = AnonymousComparer.Create<Property, string>(p => p.Id);

    public PropertyDataSource(
        Func<ICatalogRepository> repositoryFactory,
        ICategoryService categoryService,
        IPropertyService propertyService,
        IItemService productService,
        DataQuery dataQuery)
    {
        _repositoryFactory = repositoryFactory;
        _categoryService = categoryService;
        _propertyService = propertyService;
        _itemService = productService;
        _dataQuery = dataQuery ?? throw new ArgumentNullException(nameof(dataQuery));
    }

    public IEnumerable<IEntity> Items { get; set; }

    public async Task<bool> FetchAsync()
    {
        if (_fetchExecuted)
        {
            return false;
        }

        if (_dataQuery.ListEntries?.Length > 0)
        {
            Items = await GetProductPropertiesAsync(_dataQuery.ListEntries);
        }
        else
        {
            if (_dataQuery.SearchCriteria is null)
            {
                Items = [];
            }
            else
            {
                var categoryIds = _dataQuery.SearchCriteria.CategoryIds?.ToList() ?? [];
                var categories = await _categoryService.GetNoCloneAsync(categoryIds, nameof(CategoryResponseGroup.WithProperties));

                var catalogIds = _dataQuery.SearchCriteria.CatalogIds?.ToList() ?? categories.Select(x => x.CatalogId).Distinct().ToList();

                var result = new List<Property>();

                foreach (var catalogId in catalogIds)
                {
                    var catalogCategories = categories
                        .Where(x => x.CatalogId == catalogId)
                        .ToArray();

                    var properties = await GetProperties(catalogId, catalogCategories);
                    result.AddRange(properties);
                }

                Items = result;
            }
        }

        _fetchExecuted = true;

        return Items.Any();
    }

    public Task<int> GetTotalCountAsync()
    {
        throw new InvalidOperationException("GetTotalCountAsync is not supported in PropertyDataSource. Obtains all Properties in one go.");
    }

    private async Task<List<Property>> GetProductPropertiesAsync(ListEntryBase[] listEntries)
    {
        var result = new List<Property>();

        var entries = listEntries?.ToList() ?? [];

        if (entries.Count == 0)
        {
            return result;
        }

        var categoryEntries = entries
            .Where(x => x.Type.EqualsIgnoreCase(CategoryListEntry.TypeName))
            .ToArray();

        if (!categoryEntries.IsNullOrEmpty())
        {
            var categoryIds = categoryEntries.Select(x => x.Id).ToArray();
            var categories = await _categoryService.GetNoCloneAsync(categoryIds, nameof(CategoryResponseGroup.WithProperties));

            var catalogIds = categoryEntries.Select(x => x.CatalogId).Distinct().ToList();

            foreach (var catalogId in catalogIds)
            {
                var catalogCategories = categories
                    .Where(x => x.CatalogId == catalogId)
                    .ToArray();

                var properties = await GetProperties(catalogId, catalogCategories);
                result.AddRange(properties);
            }
        }

        var productIds = entries
            .Where(x => x.Type.EqualsIgnoreCase(ProductListEntry.TypeName))
            .Select(entry => entry.Id)
            .ToArray();

        if (!productIds.IsNullOrEmpty())
        {
            var propertyIds = result.Select(x => x.Id).ToHashSet();

            var products = await _itemService.GetAsync(productIds, (ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemProperties).ToString());

            // using only product inherited properties from categories,
            // own product props (only from PropertyValues) are not set via bulk update action
            var newProperties = products.SelectMany(GetPropertySelector(isInherited: true))
                .Distinct(_propertyComparer)
                .Where(x => !propertyIds.Contains(x.Id)).ToArray();

            result.AddRange(newProperties);
        }

        return result;
    }

    private async Task<List<Property>> GetProperties(string catalogId, IList<Category> catalogCategories)
    {
        if (catalogCategories.IsNullOrEmpty())
        {
            return (await GetCatalogProperties(catalogId)).ToList();
        }

        var properties = catalogCategories
            .SelectMany(GetPropertySelector())
            .ToList();

        using var repository = _repositoryFactory();

        var catalogCategoryIds = catalogCategories.Select(x => x.Id).ToArray();
        var childCategoryIds = await repository.GetAllChildrenCategoriesIdsAsync(catalogCategoryIds.ToArray());

        if (!childCategoryIds.IsNullOrEmpty())
        {
            var catalogProperties = await GetCatalogProperties(catalogId);
            var childProperties = catalogProperties.Where(x => childCategoryIds.Contains(x.CategoryId));

            properties.AddRange(childProperties);

        }

        properties = properties.Distinct(_propertyComparer).ToList();

        return properties;
    }

    private async Task<IEnumerable<Property>> GetCatalogProperties(string catalogId)
    {
        var properties = await _propertyService.GetAllCatalogPropertiesAsync(catalogId);

        return properties.Where(x => x.Type != PropertyType.Category);
    }

    private static Func<IHasProperties, IEnumerable<Property>> GetPropertySelector(bool? isInherited = null)
    {
        return isInherited == null
            ? x => x.Properties.Where(property => property.Id != null && property.Type != PropertyType.Category)
            : x => x.Properties.Where(property => property.Id != null && property.Type != PropertyType.Category && property.IsInherited == isInherited);
    }
}
