using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.BackgroundJobs;

public class AutomaticLinksJob(IAutomaticLinkService automaticLinkService)
{
    public Task UpdateLinks(string categoryId, CancellationToken cancellationToken)
        => automaticLinkService.UpdateLinks(categoryId, cancellationToken);

    [Obsolete("Hangfire compatibility shim for legacy queue items. Use the overload with CancellationToken.", DiagnosticId = "VC0014", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public Task UpdateLinks(string categoryId, IJobCancellationToken cancellationToken)
        => UpdateLinks(categoryId, cancellationToken?.ShutdownToken ?? CancellationToken.None);

    public Task DeleteLinks(string categoryId, CancellationToken cancellationToken)
        => automaticLinkService.DeleteLinks(categoryId, cancellationToken);

    [Obsolete("Hangfire compatibility shim for legacy queue items. Use the overload with CancellationToken.", DiagnosticId = "VC0014", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public Task DeleteLinks(string categoryId, IJobCancellationToken cancellationToken)
        => DeleteLinks(categoryId, cancellationToken?.ShutdownToken ?? CancellationToken.None);
}
