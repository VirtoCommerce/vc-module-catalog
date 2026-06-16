using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Services;

public interface IAutomaticLinkService
{
    [Obsolete("Use the cancellation-aware overload instead.", DiagnosticId = "VC0014", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    Task UpdateLinks(string categoryId, ICancellationToken cancellationToken);

    Task UpdateLinks(string categoryId, CancellationToken cancellationToken)
#pragma warning disable VC0014
        => UpdateLinks(categoryId, new CancellationTokenWrapper(cancellationToken));
#pragma warning restore VC0014

    [Obsolete("Use the cancellation-aware overload instead.", DiagnosticId = "VC0014", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    Task DeleteLinks(string categoryId, ICancellationToken cancellationToken);

    Task DeleteLinks(string categoryId, CancellationToken cancellationToken)
#pragma warning disable VC0014
        => DeleteLinks(categoryId, new CancellationTokenWrapper(cancellationToken));
#pragma warning restore VC0014
}
