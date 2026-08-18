using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Data.Handlers;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.CatalogModule.Data.Jobs
{
    /// <summary>
    /// Persists the change-log entries collected for changed products and categories, off the request thread.
    /// </summary>
    /// <remarks>
    /// Delegates to <see cref="LogChangesChangedEventHandler"/>, which still owns the logic - it expands the batch
    /// with the affected child categories - and stays callable by background jobs enqueued by an earlier version.
    /// </remarks>
    public class LogEntityChangesJobHandler(LogChangesChangedEventHandler eventHandler) : IBackgroundJobHandler<LogEntityChangesJobPayload>
    {
        public virtual Task Execute(LogEntityChangesJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return eventHandler.LogEntityChangesInBackgroundAsync(payload.OperationLogs);
        }
    }
}
