using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Jobs;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.CatalogModule.Data.Handlers
{
    public class LogChangesChangedEventHandler : IEventHandler<ProductChangedEvent>, IEventHandler<CategoryChangedEvent>
    {
        private readonly string _hierarchyChanged = "HierarchyChange";
        private readonly string _visibilityChanged = "VisibilityChange";

        private readonly IChangeLogService _changeLogService;
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;

        public LogChangesChangedEventHandler(IChangeLogService changeLogService,
            Func<ICatalogRepository> catalogRepositoryFactory)
        {
            _changeLogService = changeLogService;
            _catalogRepositoryFactory = catalogRepositoryFactory;
        }

        public virtual Task Handle(ProductChangedEvent @event)
        {
            return InnerHandle(@event);
        }

        public virtual Task Handle(CategoryChangedEvent @event)
        {
            return InnerHandle(@event);
        }

        // Returns Task instead of void: enqueuing is asynchronous now. Breaking for an already-compiled override,
        // which stops overriding the signature Handle calls and would be silently skipped.
        protected virtual Task InnerHandle<T>(GenericChangedEntryEvent<T> @event) where T : IEntity
        {
            var logOperations = GetLogOperations(@event.ChangedEntries).ToArray();

            var payload = AbstractTypeFactory<LogEntityChangesJobPayload>.TryCreateInstance();
            payload.OperationLogs = logOperations;

            //Background task is used here for performance reasons
            //The static facade, not an injected IBackgroundJob: RegisterEventHandler resolves this handler once from
            //the root provider and holds it for the process lifetime, so it must not capture a Scoped dependency.
            return BackgroundJob.Enqueue<LogEntityChangesJobHandler>(payload);
        }

        protected virtual IEnumerable<OperationLog> GetLogOperations<T>(IEnumerable<GenericChangedEntry<T>> changedEntries) where T : IEntity
        {
            var logOperations = changedEntries.Select(x =>
            {
                var operationLog = AbstractTypeFactory<OperationLog>.TryCreateInstance().FromChangedEntry(x);

                var hierarchyChanged = false;
                var visibilityChanged = false;

                if (x.EntryState == EntryState.Modified && x.OldEntry is Category oldCategory && x.NewEntry is Category newCategory)
                {
                    hierarchyChanged = oldCategory.CatalogId != newCategory.CatalogId ||
                                       oldCategory.ParentId != newCategory.ParentId ||
                                       oldCategory.Links?.Count != newCategory.Links?.Count;

                    visibilityChanged = oldCategory.IsActive != newCategory.IsActive;
                }

                if (hierarchyChanged)
                {
                    operationLog.Detail = _hierarchyChanged;
                }

                if (visibilityChanged)
                {
                    operationLog.Detail = _visibilityChanged;
                }

                if (x.OldEntry is CatalogProduct oldCatalogProduct)
                {
                    var variationChanged = !string.IsNullOrEmpty(oldCatalogProduct.MainProductId);

                    if (variationChanged)
                    {
                        operationLog.Detail = $"{ModuleConstants.OperationLogVariationMarker}{oldCatalogProduct.MainProductId}";
                    }
                }

                return operationLog;
            });

            return logOperations;
        }

        public async Task LogEntityChangesInBackgroundAsync(OperationLog[] operationLogs)
        {
            var result = operationLogs.ToList();

            using (var repository = _catalogRepositoryFactory())
            {
                var hierarchyLogs = await GetChildCategoriesLogs(repository, operationLogs, _hierarchyChanged);
                result.AddRange(hierarchyLogs);

                var visibilityLogs = await GetChildCategoriesLogs(repository, operationLogs, _visibilityChanged);
                result.AddRange(visibilityLogs);
            }

            await _changeLogService.SaveChangesAsync(result.ToArray());
        }

        private static async Task<List<OperationLog>> GetChildCategoriesLogs(ICatalogRepository repository, OperationLog[] operationLogs, string operationTypeMarker)
        {
            var categoryIds = operationLogs
                .Where(x => x.ObjectType == nameof(Category) && x.Detail == operationTypeMarker)
                .Select(x => x.ObjectId)
                .ToArray();

            // find affected categories
            var childCategoryIds = await repository.GetAllChildrenCategoriesIdsAsync(categoryIds);

            var result = childCategoryIds
                .Select(x =>
                {
                    var log = AbstractTypeFactory<OperationLog>.TryCreateInstance();

                    log.ObjectId = x;
                    log.ObjectType = nameof(Category);
                    log.OperationType = EntryState.Modified;
                    log.Detail = operationTypeMarker;

                    return log;
                })
                .ToList();

            return result;
        }
    }
}
