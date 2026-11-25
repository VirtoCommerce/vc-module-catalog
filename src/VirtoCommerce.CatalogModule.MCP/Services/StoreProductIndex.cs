using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.CatalogModule.MCP.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule.MCP.Services;

// A store's products in the search index, as one user may see them, in the index's own terms, so that every figure is
// one query whose cost does not grow with the catalog: a count asks for no document (Take = 0), and a list for one page.
// The products are those the storefront can reach from the store's catalog (__outline), products and not variations, of
// a catalog the user may read and of none the user may not read: the index lists a product under every catalog that
// links it, so a product of a catalog the user may not read, linked into a readable one, is left out.
public class StoreProductIndex(ISearchProvider searchProvider)
{
    public const string OutlineField = "__outline";
    public const string CatalogField = "catalog";
    public const string IsField = "is";
    public const string StatusField = "status";
    public const string StartDateField = "startdate";
    public const string EndDateField = "enddate";
    public const string ModifiedDateField = "modifieddate";
    public const string NameSortField = "__sort";

    // How many of the user's products of the store the filter matches; 0 without a catalog the user may read.
    public virtual async Task<long> CountAsync(CatalogStoreView view, IFilter filter = null, string outline = null)
    {
        if (view.ReadableCatalogs.Count == 0)
        {
            return 0;
        }

        var request = AbstractTypeFactory<SearchRequest>.TryCreateInstance();
        request.Filter = GetFilters(view, outline).Append(filter).Where(x => x != null).ToList().And();
        request.Skip = 0;
        request.Take = 0;

        var response = await searchProvider.SearchAsync(KnownDocumentTypes.Product, request);

        return response?.TotalCount ?? 0;
    }

    // One page of the user's products of the store: their ids in the index's order, and how many match.
    public virtual async Task<(IList<string> Ids, long TotalCount)> SearchAsync(CatalogStoreView view, string keyword, string outline,
        IList<SortingField> sorting, int skip, int take)
    {
        if (view.ReadableCatalogs.Count == 0)
        {
            return ([], 0);
        }

        var request = AbstractTypeFactory<SearchRequest>.TryCreateInstance();
        request.SearchKeywords = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();
        request.SearchFields = [IndexDocumentExtensions.ContentFieldName];
        request.Filter = GetFilters(view, outline).And();
        request.Sorting = sorting;
        request.Skip = skip;
        request.Take = take;
        // Only the ids: the documents may hold the whole product.
        request.IncludeFields = [ModifiedDateField];

        var response = await searchProvider.SearchAsync(KnownDocumentTypes.Product, request);

        return ((response?.Documents ?? []).Select(x => x.Id).ToList(), response?.TotalCount ?? 0);
    }

    // The products the storefront lists: visible, within their dates.
    public virtual IFilter GetListedFilter(DateTime now)
    {
        return new[]
        {
            FiltersHelper.CreateTermFilter(StatusField, "visible"),
            GetWithinDatesFilter(now),
        }.And();
    }

    // Hidden: inactive, or under an inactive category.
    public virtual IFilter GetHiddenFilter()
    {
        return FiltersHelper.CreateTermFilter(StatusField, "hidden");
    }

    // Not started yet, or ended.
    public virtual IFilter GetOutsideDatesFilter(DateTime now)
    {
        return FiltersHelper.CreateDateRangeFilter(StartDateField, now, null, false, true)
            .Or(FiltersHelper.CreateDateRangeFilter(EndDateField, null, now, false, true));
    }

    public virtual IFilter GetWithinDatesFilter(DateTime now)
    {
        // As the storefront and the catalog's own search ask for a date: started by now, ending after it.
        return FiltersHelper.CreateDateRangeFilter(StartDateField, null, now, false, true)
            .And(FiltersHelper.CreateDateRangeFilter(EndDateField, now, null, false, true));
    }

    // outline: a category's outline in the store's catalog, "catalog/category/...", or null for the whole catalog.
    protected virtual IList<IFilter> GetFilters(CatalogStoreView view, string outline)
    {
        var filters = new List<IFilter>
        {
            // The index keeps outlines in lower case.
            FiltersHelper.CreateTermFilter(OutlineField, (outline ?? view.StoreCatalog.Id).ToLowerInvariant()),
            FiltersHelper.CreateTermFilter(IsField, "product"),
            FiltersHelper.CreateTermFilter(CatalogField, view.ReadableCatalogs.Select(x => x.Id)),
        };

        if (view.HasOtherCatalogs)
        {
            filters.Add(FiltersHelper.CreateTermFilter(CatalogField, view.OtherCatalogIds).Not());
        }

        return filters;
    }
}
