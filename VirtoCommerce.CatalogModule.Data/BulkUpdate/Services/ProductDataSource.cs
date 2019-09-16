using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Services
{
    public class ProductDataSource : IPagedDataSource
    {
        private readonly ICatalogSearchService _searchService;
        private readonly IItemService _itemService;

        public ProductDataSource(ICatalogSearchService searchService, IItemService itemService, ProductBulkUpdateDataQuery dataQuery)
        {
            _searchService = searchService;
            _itemService = itemService;
            DataQuery = dataQuery ?? throw new ArgumentNullException(nameof(dataQuery));
        }

        public int PageSize { get; set; } = 50;
        public IEnumerable<IEntity> Items { get; protected set; }
        public int CurrentPageNumber { get; protected set; }

        public ProductBulkUpdateDataQuery DataQuery { get; protected set; }

        public virtual bool Fetch()
        {
            var hasObjectIds = !DataQuery.ObjectIds.IsNullOrEmpty();
            var searchCriteria = BuildSearchCriteria(DataQuery);

            if (hasObjectIds)
            {
                var objectIds = DataQuery.ObjectIds.Skip(searchCriteria.Skip).Take(searchCriteria.Take).ToArray();
                Items = _itemService.GetByIds(objectIds, ItemResponseGroup.ItemInfo);
            }
            else
            {
                var searchResult = _searchService.Search(searchCriteria);
                Items = searchResult.Products;
            }

            CurrentPageNumber++;

            return Items.Any();
        }

        public virtual int GetTotalCount()
        {
            var searchCriteria = BuildSearchCriteria(DataQuery);

            searchCriteria.Take = 0;
            searchCriteria.Skip = 0;

            var searchResult = _searchService.Search(searchCriteria);
            return searchResult.ProductsTotalCount;
        }

        protected virtual SearchCriteria BuildSearchCriteria(ProductBulkUpdateDataQuery dataQuery)
        {
            var result = AbstractTypeFactory<SearchCriteria>.TryCreateInstance();

            result.Keyword = dataQuery.Keyword;
            result.Skip = (dataQuery.Skip ?? 0) + CurrentPageNumber * PageSize;
            result.Take = dataQuery.Take ?? PageSize;

            result.CategoryIds = dataQuery.CategoryIds;
            result.CatalogIds = dataQuery.CatalogIds;

            result.ResponseGroup = SearchResponseGroup.WithProducts;

            return result;
        }
    }
}
