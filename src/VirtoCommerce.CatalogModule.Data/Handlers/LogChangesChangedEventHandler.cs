using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Data.Handlers
{
    public class LogChangesChangedEventHandler : IEventHandler<ProductChangedEvent>, IEventHandler<CategoryChangedEvent>
    {
        private readonly string _hierarchyChanged = "HeirarchyChange";

        private readonly IChangeLogService _changeLogService;
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;

        public LogChangesChangedEventHandler(IChangeLogService changeLogService, Func<ICatalogRepository> catalogRepositoryFactory)
        {
            _changeLogService = changeLogService;
            _catalogRepositoryFactory = catalogRepositoryFactory;
        }

        public virtual Task Handle(ProductChangedEvent @event)
        {
            InnerHandle(@event);
            return Task.CompletedTask;
        }

        public virtual Task Handle(CategoryChangedEvent @event)
        {
            InnerHandle(@event);
            return Task.CompletedTask;
        }

        protected virtual void InnerHandle<T>(GenericChangedEntryEvent<T> @event) where T : IEntity
        {
            var logOperations = @event.ChangedEntries.Select(x =>
            {
                var operationLog = AbstractTypeFactory<OperationLog>.TryCreateInstance().FromChangedEntry(x);

                var hierarchyChanged = false;

                if (x.OldEntry is Category oldCategory && x.NewEntry is Category newCategory)
                {
                    hierarchyChanged = oldCategory.CatalogId != newCategory.CatalogId ||
                        oldCategory.ParentId != newCategory.ParentId;
                }

                operationLog.Detail = hierarchyChanged && x.EntryState == EntryState.Modified ? _hierarchyChanged : null;

                return operationLog;
            }).ToArray();

            //Background task is used here for performance reasons
            BackgroundJob.Enqueue(() => LogEntityChangesInBackgroundAsync(logOperations));
        }

        [DisableConcurrentExecution(10)]
        // "DisableConcurrentExecutionAttribute" prevents to start simultaneous job payloads.
        // Should have short timeout, because this attribute implemented by following manner: newly started job falls into "processing" state immediately.
        // Then it tries to receive job lock during timeout. If the lock received, the job starts payload.
        // When the job is awaiting desired timeout for lock release, it stucks in "processing" anyway. (Therefore, you should not to set long timeouts (like 24*60*60), this will cause a lot of stucked jobs and performance degradation.)
        // Then, if timeout is over and the lock NOT acquired, the job falls into "scheduled" state (this is default fail-retry scenario).
        // Failed job goes to "Failed" state (by default) after retries exhausted.
        public async Task LogEntityChangesInBackgroundAsync(OperationLog[] operationLogs)
        {
            var hierarchyLogs = new List<OperationLog>();

            using (var repository = _catalogRepositoryFactory())
            {
                var categoryIds = operationLogs
                    .Where(x => x.ObjectType == nameof(Category) && x.Detail == _hierarchyChanged)
                    .Select(x => x.ObjectId)
                    .ToList();

                // find affected categories
                var childrenCategoryIds = await repository.GetAllChildrenCategoriesIdsAsync(categoryIds.ToArray());

                var categoryLogs = childrenCategoryIds.Select(x =>
                {
                    var log = AbstractTypeFactory<OperationLog>.TryCreateInstance();

                    log.ObjectId = x;
                    log.ObjectType = nameof(Category);
                    log.OperationType = EntryState.Modified;
                    log.Detail = _hierarchyChanged;

                    return log;
                });

                hierarchyLogs.AddRange(categoryLogs);

                //// find affected products
                //categoryIds.AddRange(childrenCategoryIds);
                //var childrenProductIds = await repository.Items.Where(x => categoryIds.Contains(x.CategoryId)).Select(x => x.Id).ToListAsync();

                //var productLogs = childrenProductIds.Select(x =>
                //{
                //    var log = AbstractTypeFactory<OperationLog>.TryCreateInstance();

                //    log.ObjectId = x;
                //    log.ObjectType = nameof(CatalogProduct);
                //    log.OperationType = EntryState.Modified;
                //    log.Detail = _hierarchyChanged;

                //    return log;
                //});

                //hierarchyLogs.AddRange(productLogs);
            }

            var result = operationLogs.ToList();
            result.AddRange(hierarchyLogs);

            await _changeLogService.SaveChangesAsync(result.ToArray());
        }
    }
}
