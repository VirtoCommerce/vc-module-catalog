using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.BackgroundJobs;

// The [Obsolete] IJobCancellationToken overloads are gone with the Hangfire reference: that parameter type came from
// the Hangfire package this module no longer references, so it could not be kept. Legacy queue items naming those
// overloads can no longer be dispatched; new work goes through UpdateAutomaticLinksJobHandler and
// DeleteAutomaticLinksJobHandler.
public class AutomaticLinksJob(IAutomaticLinkService automaticLinkService)
{
    public Task UpdateLinks(string categoryId, CancellationToken cancellationToken)
        => automaticLinkService.UpdateLinks(categoryId, cancellationToken);

    public Task DeleteLinks(string categoryId, CancellationToken cancellationToken)
        => automaticLinkService.DeleteLinks(categoryId, cancellationToken);
}
