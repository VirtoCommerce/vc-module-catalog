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
            using (var repository = _catalogRepositoryFactory())
            {

                if (startDate == null && endDate == null)
                {
                    // Get total products count
                    result = await repository.Items.CountAsync(i => i.ParentId == null);
                }
                else
                {
                    // Get added and modified products count
                    result = await BuildChangedItemsQuery(repository, startDate, endDate).CountAsync();
                    
                    var deletedCount = await GetTotalDeletedProductsCount(startDate, endDate);
                    result += deletedCount;
                }
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
                    result = await repository.Items
                        .Where(i => i.ParentId == null)
                        .OrderBy(i => i.CreatedDate)
                        .Select(i => ConvertItemEntityToIndexDocumentChange(i))
                        .Skip(Convert.ToInt32(skip))
                        .Take(Convert.ToInt32(take))
                        .ToListAsync();
                }
                else
                {
                    // The result is collected from two sources. The deleted ones are selected from the log of operations. Added and modified ones are selected directly from the product repository.
                    result = new List<IndexDocumentChange>();

                    var totalDeletedCount = await GetTotalDeletedProductsCount(startDate, endDate);

                    var originSkip = skip;
                    var originTake = take;

                    var deletedProductIndexDocumentChanges = await GetDeletedProductIndexDocumentChanges(startDate, endDate, originSkip, originTake);
                    var deletedCount = deletedProductIndexDocumentChanges.Count();
                    result.AddRange(deletedProductIndexDocumentChanges);

                    skip = originSkip - Math.Min(deletedCount, originSkip);
                    take = originTake - Math.Min(originTake, Math.Max(0, totalDeletedCount - originSkip));

                    var modifiedProductIndexDocumentChanges = await GetModifiedProductIndexDocumentChanges(startDate, endDate, skip, take, repository);
                    result.AddRange(modifiedProductIndexDocumentChanges);
                }
            }

            return result;
        }

        private async Task<IndexDocumentChange[]> GetDeletedProductIndexDocumentChanges(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            var criteria = new ChangeLogSearchCriteria
            {
                ObjectType = ChangeLogObjectType,
                OperationTypes = new[] { EntryState.Deleted },
                StartDate = startDate,
                EndDate = endDate,
                Skip = Convert.ToInt32(skip),
                Take = Convert.ToInt32(take)
            };

            var deleteOperations = (await _changeLogSearchService.SearchAsync(criteria)).Results;

            var deletedProductIndexDocumentChanges = deleteOperations.Select(operation =>
                new IndexDocumentChange
                {
                    DocumentId = operation.ObjectId,
                    ChangeType = IndexDocumentChangeType.Deleted,
                    ChangeDate = operation.ModifiedDate ?? operation.CreatedDate,
                }
            ).ToArray();
            return deletedProductIndexDocumentChanges;
        }

        private async Task<int> GetTotalDeletedProductsCount(DateTime? startDate, DateTime? endDate)
        {
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
            return deletedCount;
        }

        private static async Task<IndexDocumentChange[]> GetModifiedProductIndexDocumentChanges(DateTime? startDate, DateTime? endDate, long skip, long take, ICatalogRepository repository)
        {
            return await BuildChangedItemsQuery(repository, startDate, endDate)
               .OrderBy(i => i.CreatedDate)
               .Select(i => ConvertItemEntityToIndexDocumentChange(i))
               .Skip(Convert.ToInt32(skip))
               .Take(Convert.ToInt32(take))
               .ToArrayAsync();
        }

        private static IndexDocumentChange ConvertItemEntityToIndexDocumentChange(Model.ItemEntity item)
        {
            return new IndexDocumentChange
            {
                DocumentId = item.Id,
                ChangeType = IndexDocumentChangeType.Modified,
                ChangeDate = item.ModifiedDate ?? item.CreatedDate
            };
        }

        private static IQueryable<Model.ItemEntity> BuildChangedItemsQuery(ICatalogRepository repository, DateTime? startDate, DateTime? endDate)
        {
            return repository.Items.Where(i => i.ParentId == null
                && (startDate == null || i.ModifiedDate >= startDate)
                && (endDate == null || i.ModifiedDate <= endDate));
        }
    }
}
