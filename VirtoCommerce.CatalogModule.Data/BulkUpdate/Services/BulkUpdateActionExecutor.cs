using System;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Services
{
    public class BulkUpdateActionExecutor
    {
        private readonly IBulkUpdateActionRegistrar _bulkUpdateActionRegistrar;

        public BulkUpdateActionExecutor(IBulkUpdateActionRegistrar bulkUpdateActionRegistrar)
        {
            _bulkUpdateActionRegistrar = bulkUpdateActionRegistrar;
        }

        public virtual void Execute(IBulkUpdateActionContext context, Action<BulkUpdateProgressInfo> progressCallback, ICancellationToken token)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            token.ThrowIfCancellationRequested();

            var progressInfo = new BulkUpdateProgressInfo()
            {
                Description = "Validation started…",
            };
            progressCallback(progressInfo);

            var actionDefinition = _bulkUpdateActionRegistrar.GetByName(context.ActionName);
            var action = actionDefinition.Factory.Create(context);

            var validationResult = action.Validate();
            var proceed = validationResult.Succeeded;

            token.ThrowIfCancellationRequested();

            if (!proceed)
            {
                progressInfo.Description = "Validation completed with errors.";
                progressInfo.Errors = validationResult.Errors;
                progressCallback(progressInfo);
            }


            if (proceed)
            {
                var dataSourceFactory = actionDefinition.DataSourceFactory ?? throw new ArgumentException(nameof(IBulkUpdateActionDefinition.DataSourceFactory));
                var dataSource = dataSourceFactory.Create(context.DataQuery);

                progressInfo.ProcessedCount = 0;
                progressInfo.TotalCount = dataSource.GetTotalCount();
                progressInfo.Description = "Bulk update started…";
                progressCallback(progressInfo);

                // Paginated execution progress need to be added

                while (dataSource.Fetch())
                {
                    token.ThrowIfCancellationRequested();
                    action.Execute(dataSource.Items);
                }
            }



        }
    }
}
