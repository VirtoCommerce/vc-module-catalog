using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogModule2.Data.Search.Indexing
{
    public class ProductDocumentChangesProvider2 : ProductDocumentChangesProvider
    {
        public ProductDocumentChangesProvider2(Func<ICatalogRepository> catalogRepositoryFactory, IChangeLogSearchService changeLogSearchService) : base(catalogRepositoryFactory, changeLogSearchService)
        {
        }
        public override Task<IList<IndexDocumentChange>> GetChangesAsync(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            return base.GetChangesAsync(startDate, endDate, skip, take);
        }
        public override Task<long> GetTotalChangesCountAsync(DateTime? startDate, DateTime? endDate)
        {
            return base.GetTotalChangesCountAsync(startDate, endDate);
        }
    }
}
