using System;
using System.Linq;
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

            var totalCount = 0;
            var processedCount = 0;

            var progressInfo = new BulkUpdateProgressInfo()
            {
                Description = "Validation has started…",
            };
            progressCallback(progressInfo);

            try
            {
                var actionDefinition = _bulkUpdateActionRegistrar.GetByName(context.ActionName);
                var action = actionDefinition.Factory.Create(context);

                var validationResult = action.Validate();
                var proceed = validationResult.Succeeded;

                token.ThrowIfCancellationRequested();

                if (!proceed)
                {
                    progressInfo.Description = "Validation completed with errors.";
                    progressInfo.Errors = validationResult.Errors;
                }
                else
                {
                    progressInfo.Description = "Validation completed successfully.";
                }

                progressCallback(progressInfo);

                if (proceed)
                {
                    var dataSourceFactory = actionDefinition.DataSourceFactory ?? throw new ArgumentException(nameof(IBulkUpdateActionDefinition.DataSourceFactory));
                    var dataSource = dataSourceFactory.Create(context.DataQuery);
                    totalCount = dataSource.GetTotalCount();
                    processedCount = 0;

                    progressInfo.ProcessedCount = processedCount;
                    progressInfo.TotalCount = totalCount;
                    progressInfo.Description = "Update has started…";
                    progressCallback(progressInfo);

                    while (dataSource.Fetch())
                    {
                        token.ThrowIfCancellationRequested();

                        var result = action.Execute(dataSource.Items);

                        if (!result.Succeeded)
                        {
                            progressInfo.Errors.AddRange(result.Errors);
                        }

                        processedCount += dataSource.Items.Count();

                        if (processedCount < totalCount)
                        {
                            progressInfo.Description = $"{processedCount} out of {totalCount} have been updated.";
                            progressCallback(progressInfo);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                progressInfo.Errors.Add(e.Message);
            }
            finally
            {
                var completedMessage = (progressInfo.Errors?.Count > 0) ? "Update completed with errors" : "Update completed";

                progressInfo.Description = $"{completedMessage}: {processedCount} out of {totalCount} have been updated.";
                progressCallback(progressInfo);
            }
        }
    }
}
