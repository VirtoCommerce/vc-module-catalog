using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Extensions;
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

                    var variationChangesCount = (await GetVariationIndexDocumentChangesAsync(startDate, endDate)).Count();
                    result += variationChangesCount;
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
                        .Select(i => new IndexDocumentChange
                        {
                            DocumentId = i.Id,
                            ChangeType = IndexDocumentChangeType.Modified,
                            ChangeDate = i.ModifiedDate ?? i.CreatedDate
                        })
                        .Skip(Convert.ToInt32(skip))
                        .Take(Convert.ToInt32(take))
                        .ToListAsync();
                }
                else
                {
                    // The result is collected from two sources. The deleted ones are selected from the log of operations. Added and modified ones are selected directly from the product repository.
                    result = new List<IndexDocumentChange>();

                    var originSkip = skip;
                    var originTake = take;

                    var searchResult = await SearchDeleteOperationsInLog(startDate, endDate, originSkip, originTake);
                    var totalDeletedCount = searchResult.TotalCount;
                    var deletedProductIndexDocumentChanges = searchResult.Results.Select(ConvertOperationLogToIndexDocumentChange).ToArray();
                    result.AddRange(deletedProductIndexDocumentChanges);

                    skip = originSkip - Math.Min(totalDeletedCount, originSkip);
                    take = originTake - Math.Min(originTake, Math.Max(0, totalDeletedCount - originSkip));

                    var modifiedAndCreatedProductIndexDocumentChanges = await GetModifiedAndCreatedProductIndexDocumentChanges(startDate, endDate, skip, take, repository);
                    result.AddRange(modifiedAndCreatedProductIndexDocumentChanges);

                    var changedVariationIndexDocumentChanges = await GetVariationIndexDocumentChangesAsync(startDate, endDate);
                    result.AddRange(changedVariationIndexDocumentChanges);
                }
            }

            return result;
        }

        private async Task<ChangeLogSearchResult> SearchDeleteOperationsInLog(DateTime? startDate, DateTime? endDate, long skip, long take)
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

            var searchResult = await _changeLogSearchService.SearchAsync(criteria);

            return searchResult;
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

            var deletedOperationsResult = await _changeLogSearchService.SearchAsync(criteria);
            var deletedCount = deletedOperationsResult.TotalCount;
            return deletedCount;
        }

        private async Task<IndexDocumentChange[]> GetModifiedAndCreatedProductIndexDocumentChanges(DateTime? startDate, DateTime? endDate, long skip, long take, ICatalogRepository repository)
        {
            var result = await BuildChangedItemsQuery(repository, startDate, endDate)
               .OrderBy(i => i.CreatedDate)
               .Select(i => new IndexDocumentChange
               {
                   DocumentId = i.Id,
                   ChangeType = IndexDocumentChangeType.Modified,
                   ChangeDate = i.ModifiedDate ?? i.CreatedDate
               })
               .Skip(Convert.ToInt32(skip))
               .Take(Convert.ToInt32(take))
               .ToArrayAsync();

            var changeLogSearchCriteria = new ChangeLogSearchCriteria
            {
                StartDate = startDate,
                EndDate = endDate,
                OperationTypes = new[] { EntryState.Added },
                ObjectType = ChangeLogObjectType,
                Take = result.Length,
            };

            var changeLogSearchResult = await _changeLogSearchService.SearchAsync(changeLogSearchCriteria);
            var addedItems = changeLogSearchResult.Results;

            if (addedItems.Any())
            {
                foreach (var addedItem in addedItems)
                {
                    var resultItem = result.FirstOrDefault(x => x.DocumentId == addedItem.ObjectId);

                    if (resultItem != null)
                    {
                        resultItem.ChangeType = IndexDocumentChangeType.Created;
                    }
                }
            }

            return result.ToArray();
        }

        private async Task<IEnumerable<IndexDocumentChange>> GetVariationIndexDocumentChangesAsync(DateTime? startDate, DateTime? endDate)
        {
            var result = new List<IndexDocumentChange>();

            var changeLogSearchCriteria = new ChangeLogSearchCriteria
            {
                StartDate = startDate,
                EndDate = endDate,
                ObjectType = ChangeLogObjectType,
                OperationTypes = new[] { EntryState.Deleted, EntryState.Added, EntryState.Modified },
                Take = 0,
            };

            var changeLogSearchResult = await _changeLogSearchService.SearchAsync(changeLogSearchCriteria);

            if (changeLogSearchResult.TotalCount > 0)
            {
                changeLogSearchCriteria.Take = changeLogSearchResult.TotalCount;

                changeLogSearchResult = await _changeLogSearchService.SearchAsync(changeLogSearchCriteria);

                var changedVariations = changeLogSearchResult
                    .Results
                    .Where(x => x.Detail?.StartsWith(ModuleConstants.OperationLogVariationMarker) ?? false).ToList();

                foreach (var changedVariation in changedVariations)
                {
                    result.AddRange(new[] {

                        // Main product change
                        new IndexDocumentChange
                        {
                            DocumentId = changedVariation.Detail[ModuleConstants.OperationLogVariationMarker.Length..],
                            ChangeType = IndexDocumentChangeType.Modified,
                            ChangeDate = changedVariation.CreatedDate,
                        },

                        // Variation change
                        new IndexDocumentChange
                        {
                            DocumentId = changedVariation.ObjectId,
                            ChangeType = changedVariation.OperationType.ToIndexDocumentChangeType(),
                            ChangeDate = changedVariation.CreatedDate,
                        }
                    });
                }
            }

            return result;
        }

        private static IndexDocumentChange ConvertOperationLogToIndexDocumentChange(OperationLog operation)
        {
            return new IndexDocumentChange
            {
                DocumentId = operation.ObjectId,
                ChangeType = IndexDocumentChangeType.Deleted,
                ChangeDate = operation.ModifiedDate ?? operation.CreatedDate,
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
