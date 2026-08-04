using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Data.BackgroundJobs;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.CatalogModule.Data.Jobs
{
    /// <summary>
    /// Rebuilds a category's automatic links, off the request thread.
    /// </summary>
    public class UpdateAutomaticLinksJobHandler(AutomaticLinksJob job) : IBackgroundJobHandler<AutomaticLinksJobPayload>
    {
        public virtual Task Execute(AutomaticLinksJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return job.UpdateLinks(payload.CategoryId, cancellationToken);
        }
    }
}
