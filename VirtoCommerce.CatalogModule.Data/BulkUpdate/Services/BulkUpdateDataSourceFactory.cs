using System;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.ChangeCategory;
using VirtoCommerce.CatalogModule.Web.Services;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Services
{
    public class BulkUpdateDataSourceFactory : IPagedDataSourceFactory
    {
        private readonly IListEntrySearchService _searchService;

        public BulkUpdateDataSourceFactory(IListEntrySearchService searchService)
        {
            _searchService = searchService;
        }

        public IPagedDataSource Create(BulkUpdateActionContext context)
        {
            IPagedDataSource result = null;

            if (context is ChangeCategoryActionContext changeCategoryActionContext)
            {
                result = new ListEntryPagedDataSource(_searchService, changeCategoryActionContext.DataQuery);
            }

            return result ?? throw new ArgumentException($"Unsupported bulk update query type: {context.GetType().Name}");
        }
    }
}
