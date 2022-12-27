using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Search
{
    public class ListEntrySearchService2 : ListEntrySearchService
    {
        public ListEntrySearchService2(Func<ICatalogRepository> catalogRepositoryFactory, IItemService itemService, ICategoryService categoryService) : base(catalogRepositoryFactory, itemService, categoryService)
        {
        }
        protected override IQueryable<CategoryEntity> BuildQuery(IQueryable<CategoryEntity> query, CatalogListEntrySearchCriteria criteria)
        {
            return base.BuildQuery(query, criteria);
        }
        protected override IQueryable<ItemEntity> BuildQuery(IQueryable<ItemEntity> query, CatalogListEntrySearchCriteria criteria, string[] searchCategoryIds)
        {
            return base.BuildQuery(query, criteria, searchCategoryIds);
        }
        protected override IList<SortInfo> BuildSortExpression(CatalogListEntrySearchCriteria criteria)
        {
            return base.BuildSortExpression(criteria);
        }
        protected override Task<GenericSearchResult<Category>> SearchCategoriesAsync(CatalogListEntrySearchCriteria criteria)
        {
            return base.SearchCategoriesAsync(criteria);
        }
        protected override Task<GenericSearchResult<CatalogProduct>> SearchItemsAsync(CatalogListEntrySearchCriteria criteria)
        {
            return base.SearchItemsAsync(criteria);
        }
        protected override void TryTransformSortingInfoColumnNames(IDictionary<string, string> transformationMap, IEnumerable<SortInfo> sortingInfos)
        {
            base.TryTransformSortingInfoColumnNames(transformationMap, sortingInfos);
        }
    }
}
