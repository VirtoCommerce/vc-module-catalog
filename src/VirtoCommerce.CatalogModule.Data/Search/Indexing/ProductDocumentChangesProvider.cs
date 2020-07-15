using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing
{
    public class ProductDocumentChangesProvider : IIndexDocumentChangesProvider
    {
        public const string ChangeLogObjectType = nameof(CatalogProduct);

        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly IChangeLogSearchService _changeLogSearchService;

        public ProductDocumentChangesProvider(Func<ICatalogRepository> catalogRepositoryFactory, IChangeLogSearchService changeLogSearchService)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _changeLogSearchService = changeLogSearchService;
        }

        public virtual async Task<long> GetTotalChangesCountAsync(DateTime? startDate, DateTime? endDate)
        {
            long result;

            if (startDate == null && endDate == null)
            {
                // Get total products count
                using (var repository = _catalogRepositoryFactory())
                {
                    result = await repository.Items.CountAsync(i => i.ParentId == null);
                }
            }
            else
            {
                // Get added and modified products count
                using (var repository = _catalogRepositoryFactory())
                {
                    result = await GetChangedItemsQuery(repository, startDate, endDate).CountAsync();
                }

                var criteria = new ChangeLogSearchCriteria
                {
                    ObjectType = ChangeLogObjectType,
                    OperationTypes = new[] { EntryState.Deleted },
                    StartDate = startDate,
                    EndDate = endDate,
                    Take = 0
                };
                
                var deletedOperations = (await _changeLogSearchService.SearchAsync(criteria)).Results;
                var deletedCount = deletedOperations.Count;
                result += deletedCount;
            }

            return result;
        }

        public virtual async Task<IList<IndexDocumentChange>> GetChangesAsync(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            IList<IndexDocumentChange> result;
            // Get documents from repository and return them as changes
            using (var repository = _catalogRepositoryFactory())
            {

                if (startDate == null && endDate == null)
                {
                    var changedProductsInfos = await repository.Items
                        .Where(i => i.ParentId == null)
                        .OrderBy(i => i.CreatedDate)
                        .Select(i => new { i.Id, i.CreatedDate, i.ModifiedDate })
                        .Skip((int)skip)
                        .Take((int)take)
                        .ToArrayAsync();

                    result = changedProductsInfos.Select(itemInfo =>
                        new IndexDocumentChange
                        {
                            DocumentId = itemInfo.Id,
                            ChangeType = IndexDocumentChangeType.Modified,
                            ChangeDate = itemInfo.ModifiedDate ?? itemInfo.CreatedDate
                        }
                    ).ToArray();
                }
                else
                {
                    var changedProductsInfos = await GetChangedItemsQuery(repository, startDate, endDate)
                       .OrderBy(i => i.CreatedDate)
                       .Select(i => new { i.Id, i.CreatedDate, i.ModifiedDate })
                       .Skip((int)skip)
                       .Take((int)take)
                       .ToArrayAsync();

                    var changedProductIdsList = changedProductsInfos.Select(itemInfo =>
                        new IndexDocumentChange
                        {
                            DocumentId = itemInfo.Id,
                            ChangeType = IndexDocumentChangeType.Modified,
                            ChangeDate = itemInfo.ModifiedDate ?? itemInfo.CreatedDate
                        }
                    ).ToList();

                    // Get changes from operation log for deleted items
                    var criteria = new ChangeLogSearchCriteria
                    {
                        ObjectType = ChangeLogObjectType,
                        OperationTypes = new[] { EntryState.Deleted },
                        StartDate = startDate,
                        EndDate = endDate,
                        Skip = (int)skip,
                        Take = (int)take
                    };

                    var operations = (await _changeLogSearchService.SearchAsync(criteria)).Results;

                    var deletedProductIndexDocumentChanges = operations.Select(o =>
                        new IndexDocumentChange
                        {
                            DocumentId = o.ObjectId,
                            ChangeType = IndexDocumentChangeType.Deleted,
                            ChangeDate = o.ModifiedDate ?? o.CreatedDate,
                        }
                    ).ToArray();

                    changedProductIdsList.AddRange(deletedProductIndexDocumentChanges);
                    result = changedProductIdsList.ToArray();
                }
            }

            return result;
        }

        private  IQueryable<Model.ItemEntity> GetChangedItemsQuery(ICatalogRepository repository, DateTime? startDate, DateTime? endDate)
        {
            return repository.Items.Where(i => i.ParentId == null && (i.ModifiedDate >= startDate || startDate == null) && (i.ModifiedDate <= endDate || endDate == null));
        }
    }
}
