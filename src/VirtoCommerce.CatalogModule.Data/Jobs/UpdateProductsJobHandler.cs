using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Data.Handlers;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.CatalogModule.Data.Jobs
{
    /// <summary>
    /// Resaves the products under categories whose hierarchy or visibility changed, off the request thread.
    /// </summary>
    /// <remarks>
    /// Delegates to <see cref="TrackSpecialChangesEventHandler"/>, which still owns the logic and stays callable by
    /// background jobs enqueued by an earlier version that reference its method by name.
    /// </remarks>
    public class UpdateProductsJobHandler(TrackSpecialChangesEventHandler eventHandler) : IBackgroundJobHandler<UpdateProductsJobPayload>
    {
        public virtual Task Execute(UpdateProductsJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return eventHandler.UpdateProductsAsync(payload.CategoryIds);
        }
    }
}
