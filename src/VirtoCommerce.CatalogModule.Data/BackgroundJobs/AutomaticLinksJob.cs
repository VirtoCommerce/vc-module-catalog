using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Hangfire;

namespace VirtoCommerce.CatalogModule.Data.BackgroundJobs;

public class AutomaticLinksJob(IAutomaticLinkService automaticLinkService)
{
    public Task UpdateLinks(string categoryId, IJobCancellationToken cancellationToken)
    {
        return automaticLinkService.UpdateLinks(categoryId, new JobCancellationTokenWrapper(cancellationToken));
    }
}
