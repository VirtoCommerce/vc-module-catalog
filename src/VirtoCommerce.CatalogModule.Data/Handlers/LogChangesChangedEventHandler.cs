using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.CatalogModule.Core;
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
            var logOperations = GetLogOperations(@event.ChangedEntries).ToArray();

            //Background task is used here for performance reasons
            BackgroundJob.Enqueue(() => LogEntityChangesInBackgroundAsync(logOperations));
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
